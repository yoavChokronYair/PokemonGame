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
    }
}