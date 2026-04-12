using PokemonGame.Core.Config;

namespace PokemonGame.Model.Domain.Pokemon
{
    public sealed class PokemonTeam
    {

        private readonly PokemonState[] _slots;
        private int _activeIndex;

        private PokemonTeam(IReadOnlyList<PokemonState> roster)
        {
            if (roster.Count != PokemonConstants.PartyCapacity)
            {
                throw new ArgumentException(
                    $"A team must have exactly {PokemonConstants.PartyCapacity} Pokémon, got {roster.Count}.");
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
    }
}
