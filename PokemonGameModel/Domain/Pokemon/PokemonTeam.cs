using PokemonGame.Core.Config;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Pokemon
{
    public sealed class PokemonTeam
    {

        private readonly PokemonState[] _slots;
        private int _activeIndex;

        private PokemonTeam(IReadOnlyList<PokemonState> roster)
        {
            // Fix: Check that the roster is between 1 and the maximum allowed capacity (6)
            if (roster == null || roster.Count == 0)
            {
                throw new ArgumentException("A team must have at least 1 Pokémon.");
            }

            if (roster.Count > PokemonConstants.PartyCapacity)
            {
                throw new ArgumentException(
                    $"A team cannot exceed {PokemonConstants.PartyCapacity} Pokémon, got {roster.Count}.");
            }

            _slots = roster.ToArray();
            _activeIndex = 0;
        }
        public PokemonState GetPokemonAt(int index) => _slots[index];


        /// <summary>
        /// Call once per battle side with whatever roster was built by the ViewModel.
        /// </summary>
        public static PokemonTeam Create(IReadOnlyList<PokemonState> roster)
            => new PokemonTeam(roster);

        // ── Active slot ───────────────────────────────────────────────────────

        public PokemonState Active => _slots[_activeIndex];
        public int ActiveIndex => _activeIndex;

        // ── Team view ─────────────────────────────────────────────────────────

        private IReadOnlyList<PokemonState> All => _slots;
        private IEnumerable<PokemonState> Alive => _slots.Where(s => !s.IsFainted);
        public bool IsDefeated => _slots.All(s => s.IsFainted);
        public int GetAlivePokemonCount() => Alive.Count();
        public int getAllPokemonCount() => All.Count();

        public bool ContainsPokemon(int pokedexId)
        {
            return _slots.Any(p => p.PokedexId == pokedexId);
        }
        public int GetPokemonIndex(int pokedexId)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].PokedexId == pokedexId)
                    return i;
            }

            return -1; // not found
        }
        //TODO:add proper evolution handling here, this is just a placeholder to get the flow working
        public bool TradePokemon(PokemonState currentPokemon, PokemonState newPokemon)
        {
            if (currentPokemon == null || newPokemon == null)
                return false;

            int index = Array.IndexOf(_slots, currentPokemon);

            if (index == -1)
                return false;

            _slots[index] = newPokemon;

            FixActiveAfterTrade();
            if(newPokemon.Evolution.TriggerType == EvoTriggerType.Trade)
            {

            }
            return true;
        }
        private void FixActiveAfterTrade()
        {
            if (_slots[_activeIndex] != null && !_slots[_activeIndex].IsFainted)
                return;

            for (int i = 0; i < PokemonConstants.PartyCapacity; i++)
            {
                if (_slots[i] != null && !_slots[i].IsFainted)
                {
                    _activeIndex = i;
                    return;
                }
            }

            _activeIndex = 0;
        }

        // ── Switching ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns false (no change) when: out of range, already active, or fainted.
        /// Caller should re-prompt the player/bot to pick again on false.
        /// </summary>
        public bool SwitchTo(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= PokemonConstants.PartyCapacity)
            {
                return false;
            }

            if (slotIndex == _activeIndex)
            {
                return false;
            }

            if (_slots[slotIndex].IsFainted)
            {
                return false;
            }
            _slots[_activeIndex].turnsActive = 0; // reset turns active for the old active Pokémon
            _activeIndex = slotIndex;
            _slots[_activeIndex].turnsActive = 0;
            return true;
        }

        /// <summary>
        /// Auto-picks the first alive non-active slot.
        /// Call this when the active Pokémon faints and a forced switch is needed.
        /// Returns false only if the whole team is defeated.
        /// </summary>
        public bool SwitchToNextAvailable()
        {
            for (int i = 0; i < PokemonConstants.PartyCapacity; i++)
            {
                if (i != _activeIndex && !_slots[i].IsFainted)
                {
                    _slots[_activeIndex].turnsActive = 0; // reset turns active for the old active Pokémon
                    _activeIndex = i;
                    _slots[_activeIndex].turnsActive = 0; // reset turns active for the old active Pokémon
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns slot indices the player/bot can legally switch to right now.
        /// Feed this to switch UI or bot decision logic.
        /// </summary>
        public IReadOnlyList<int> GetSwitchableIndices()
        {
            var result = new List<int>();
            for (int i = 0; i < PokemonConstants.PartyCapacity; i++)
            {
                if (i != _activeIndex && !_slots[i].IsFainted)
                {
                    result.Add(i);
                }
            }
            return result;
        }

        public override string ToString()
        {
            var lines = _slots.Select((s, i) =>
                $"  [{i}]{(i == _activeIndex ? "*" : " ")} {s}");
            return $"Team:\n{string.Join("\n", lines)}";
        }
        public bool AnyPokemonKnows(string moveName)
        {
            return _slots.Any(s => s.Moves.Any(m => ((MoveState)m).Name == moveName));
        }
        public void HealAll()
        {
            foreach (var pokemon in _slots)
            {
                if (pokemon == null) continue;

                pokemon.CurrentHP = pokemon.MaxHP;

                // Clear status
                pokemon.ClearStatus();

                // Reset battle-only state (important!)
                pokemon.ResetStatStages();
                pokemon.VolatileStatuses.Clear();

                // Reset temporary combat flags
                pokemon.LastDamageDealt = 0;
                pokemon.LastDamageTaken = 0;

                // Reset multipliers (optional but recommended)
                // You don’t have direct reset methods, so either:
                // 1. add a ResetMultipliers() method
                // OR
                // 2. leave as-is if they only matter in battle context

                // Reset turns active
                pokemon.turnsActive = 0;

                // TODO: restore PP when you implement it
                // pokemon.RestoreAllPP();
            }
        }
    }
}
