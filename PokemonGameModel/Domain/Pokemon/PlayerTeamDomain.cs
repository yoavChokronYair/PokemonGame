using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Pokemon
{
    /// <summary>
    /// Represents the player's active team of up to 6 Pokémon.
    /// Pure data — no battle logic.
    /// </summary>
    public class PlayerTeamDomain
    {
        // ─── Constants ────────────────────────────────────────────────────────

        public const int MaxTeamSize = 6;


        // ─── Team Slots ───────────────────────────────────────────────────────

        /// <summary>
        /// The six team slots in order. A slot is null if empty.
        /// Slot 0 is always the lead Pokémon.
        /// </summary>
        private readonly PokemonPlayerDomain?[] _slots = new PokemonPlayerDomain?[MaxTeamSize];
        public IReadOnlyList<PokemonPlayerDomain> Members => _slots;

        /// <summary>Read-only view of all slots (including nulls)</summary>
        public IReadOnlyList<PokemonPlayerDomain?> Slots => _slots;

        /// <summary>Only the filled slots, in order</summary>
        public IEnumerable<PokemonPlayerDomain> ActiveMembers => _slots.Where(p => p != null)!;

        /// <summary>Number of Pokémon currently on the team</summary>
        public int Count => _slots.Count(p => p != null);

        /// <summary>Whether the team is full</summary>
        public bool IsFull => Count >= MaxTeamSize;

        /// <summary>The current lead Pokémon (slot 0). Null if team is empty.</summary>
        public PokemonPlayerDomain? Lead => _slots[0];

        /// <summary>All non-fainted members in order</summary>
        public IEnumerable<PokemonPlayerDomain> HealthyMembers => ActiveMembers.Where(p => !p.IsFainted);

        /// <summary>Whether all Pokémon on the team have fainted (blackout condition)</summary>
        public bool IsBlackedOut => ActiveMembers.Any() && ActiveMembers.All(p => p.IsFainted);


        // ─── Team Manipulation ────────────────────────────────────────────────

        /// <summary>
        /// Adds a Pokémon to the first empty slot.
        /// Returns true on success, false if the team is full.
        /// </summary>
        public bool TryAdd(PokemonPlayerDomain pokemon)
        {
            for (int i = 0; i < MaxTeamSize; i++)
            {
                if (_slots[i] == null)
                {
                    _slots[i] = pokemon;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes a Pokémon by its UID.
        /// Returns true if found and removed.
        /// </summary>
        public bool TryRemove(int pokemonUID)
        {
            for (int i = 0; i < MaxTeamSize; i++)
            {
                if (_slots[i]?.PokemonUID == pokemonUID)
                {
                    _slots[i] = null;
                    ShiftSlotsLeft();
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Swaps two Pokémon by their slot indices (0–5).
        /// Useful for reordering the team.
        /// </summary>
        public void SwapSlots(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= MaxTeamSize) throw new ArgumentOutOfRangeException(nameof(indexA));
            if (indexB < 0 || indexB >= MaxTeamSize) throw new ArgumentOutOfRangeException(nameof(indexB));

            (_slots[indexA], _slots[indexB]) = (_slots[indexB], _slots[indexA]);
        }

        /// <summary>
        /// Moves a Pokémon to slot 0, making it the new lead.
        /// </summary>
        public void SetLead(int pokemonUID)
        {
            for (int i = 1; i < MaxTeamSize; i++)
            {
                if (_slots[i]?.PokemonUID == pokemonUID)
                {
                    SwapSlots(0, i);
                    return;
                }
            }
        }

        /// <summary>
        /// Returns the Pokémon at a given slot index, or null if empty.
        /// </summary>
        public PokemonPlayerDomain? GetAt(int index)
        {
            if (index < 0 || index >= MaxTeamSize) throw new ArgumentOutOfRangeException(nameof(index));
            return _slots[index];
        }

        /// <summary>
        /// Returns the first non-fainted member, or null if all have fainted.
        /// </summary>
        public PokemonPlayerDomain? GetFirstHealthy() => HealthyMembers.FirstOrDefault();

        /// <summary>
        /// Heals all Pokémon to full HP and clears their status (e.g. after a Pokémon Centre visit).
        /// </summary>
        public void FullHealAll()
        {
            foreach (var pokemon in ActiveMembers)
            {
                pokemon.CurrentHP = pokemon.PokemonState.MaxHP;
                pokemon.PersistentStatus = StatusCondition.None;

                foreach (var move in pokemon.Moves)
                    if (move != null) move.PP = move.MaxPP;
            }
        }


        // ─── Helpers ──────────────────────────────────────────────────────────

        /// <summary>Compacts the array so there are no gaps between filled slots.</summary>
        private void ShiftSlotsLeft()
        {
            int write = 0;
            for (int read = 0; read < MaxTeamSize; read++)
            {
                if (_slots[read] != null)
                    _slots[write++] = _slots[read];
            }
            for (int i = write; i < MaxTeamSize; i++)
                _slots[i] = null;
        }
        /// <summary>
        /// Returns true if any Pokémon on the team knows the specified move.
        /// </summary>
        public bool AnyPokemonKnows(string moveName)
        {
            return ActiveMembers.Any(p =>
                p.Moves.Any(m => m != null && m.Name == moveName));
        }

        /// <summary>
        /// Returns true if any Pokémon on the team matches the given Pokédex ID.
        /// </summary>
        public bool ContainsPokemon(int pokedexId)
        {
            return ActiveMembers.Any(p => p.PokedexId == pokedexId);
        }

        /// <summary>
        /// Returns the slot index of the first Pokémon matching the given Pokédex ID,
        /// or -1 if not found.
        /// </summary>
        public int GetPokemonIndex(int pokedexId)
        {
            for (int i = 0; i < MaxTeamSize; i++)
            {
                if (_slots[i]?.PokedexId == pokedexId)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Returns the Pokémon at the given slot index.
        /// Throws if the index is out of range or the slot is empty.
        /// </summary>
        public PokemonPlayerDomain GetPokemonAt(int index)
        {
            if (index < 0 || index >= MaxTeamSize)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _slots[index] ?? throw new InvalidOperationException(
                $"Slot {index} is empty.");
        }


        // ─── Trade ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Replaces <paramref name="currentPokemon"/> with <paramref name="newPokemon"/>
        /// in the same slot. Returns false if either argument is null or the Pokémon
        /// is not on the team.
        /// TODO: add proper evolution handling (trigger on trade).
        /// </summary>
        public bool TradePokemon(PokemonPlayerDomain currentPokemon, PokemonPlayerDomain newPokemon)
        {
            if (currentPokemon == null || newPokemon == null)
                return false;

            for (int i = 0; i < MaxTeamSize; i++)
            {
                if (_slots[i] == currentPokemon)
                {
                    _slots[i] = newPokemon;
                    // TODO: if (newPokemon.Evolution.TriggerType == EvoTriggerType.Trade) { ... }
                    return true;
                }
            }
            return false;
        }


        // ─── Heal ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Fully restores HP, clears status, and restores all move PP for every
        /// Pokémon on the team. Mirrors PokemonTeam.HealAll().
        /// </summary>
        public void HealAll()
        {
            foreach (var pokemon in ActiveMembers)
            {
                pokemon.CurrentHP = pokemon.PokemonState.MaxHP;
                pokemon.PersistentStatus = StatusCondition.None;

                pokemon.ResetStatStages();
                pokemon.VolatileStatuses.Clear();

                pokemon.LastDamageDealt = 0;
                pokemon.LastDamageTaken = 0;
                pokemon.turnsActive = 0;

                foreach (var move in pokemon.Moves)
                    if (move != null) move.PP = move.MaxPP;
            }
        }
    }
}