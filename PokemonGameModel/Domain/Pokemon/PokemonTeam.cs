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

        public static PokemonTeam Create(IReadOnlyList<PokemonState> roster)
            => new PokemonTeam(roster);

        public IReadOnlyList<PokemonState> Members => _slots;

        public PokemonState Active => _slots[_activeIndex];

        public int ActiveIndex => _activeIndex;

        public int Count => _slots.Length;

        private IReadOnlyList<PokemonState> All => _slots;

        private IEnumerable<PokemonState> Alive =>
            _slots.Where(pokemon => pokemon != null && !pokemon.IsFainted);

        public bool IsDefeated =>
            _slots.All(pokemon => pokemon == null || pokemon.IsFainted);

        public int GetAlivePokemonCount() => Alive.Count();

        public int getAllPokemonCount() => _slots.Length;

        public PokemonState GetPokemonAt(int index)
        {
            if (!IsValidIndex(index))
                throw new ArgumentOutOfRangeException(nameof(index));

            return _slots[index];
        }

        public bool ContainsPokemon(int pokedexId)
        {
            return _slots.Any(pokemon =>
                pokemon != null &&
                pokemon.PokedexId == pokedexId);
        }

        public int GetPokemonIndex(int pokedexId)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null && _slots[i].PokedexId == pokedexId)
                    return i;
            }

            return -1;
        }

        public bool TradePokemon(PokemonState currentPokemon, PokemonState newPokemon)
        {
            if (currentPokemon == null || newPokemon == null)
                return false;

            int index = Array.IndexOf(_slots, currentPokemon);

            if (index < 0)
                return false;

            _slots[index] = newPokemon;

            if (_activeIndex == index)
                _activeIndex = index;

            FixActiveAfterTrade();

            TryApplyTradeEvolution(newPokemon);

            return true;
        }

        private void TryApplyTradeEvolution(PokemonState pokemon)
        {
            if (pokemon == null)
                return;

            if (pokemon.Evolution == null)
                return;

            if (pokemon.Evolution.TriggerType != EvoTriggerType.Trade)
                return;

            // TODO:
            // Add your real evolution service/factory here.
            // For now we only keep the safe detection point.
            //
            // Example future flow:
            // pokemon.EvolveTo(pokemon.Evolution.TargetPokemonId);
        }

        public void SwapSlots(int indexA, int indexB)
        {
            if (!IsValidIndex(indexA))
                throw new ArgumentOutOfRangeException(nameof(indexA));

            if (!IsValidIndex(indexB))
                throw new ArgumentOutOfRangeException(nameof(indexB));

            if (indexA == indexB)
                return;

            (_slots[indexA], _slots[indexB]) = (_slots[indexB], _slots[indexA]);

            if (_activeIndex == indexA)
                _activeIndex = indexB;
            else if (_activeIndex == indexB)
                _activeIndex = indexA;
        }

        private void FixActiveAfterTrade()
        {
            if (IsValidIndex(_activeIndex) &&
                _slots[_activeIndex] != null &&
                !_slots[_activeIndex].IsFainted)
            {
                return;
            }

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] != null && !_slots[i].IsFainted)
                {
                    _activeIndex = i;
                    return;
                }
            }

            _activeIndex = 0;
        }

        public bool SwitchTo(int slotIndex)
        {
            if (!IsValidIndex(slotIndex))
                return false;

            if (slotIndex == _activeIndex)
                return false;

            PokemonState target = _slots[slotIndex];

            if (target == null || target.IsFainted)
                return false;

            if (IsValidIndex(_activeIndex) && _slots[_activeIndex] != null)
                _slots[_activeIndex].turnsActive = 0;

            _activeIndex = slotIndex;
            _slots[_activeIndex].turnsActive = 0;

            return true;
        }

        public bool SwitchToNextAvailable()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (i == _activeIndex)
                    continue;

                PokemonState pokemon = _slots[i];

                if (pokemon == null || pokemon.IsFainted)
                    continue;

                if (IsValidIndex(_activeIndex) && _slots[_activeIndex] != null)
                    _slots[_activeIndex].turnsActive = 0;

                _activeIndex = i;
                _slots[_activeIndex].turnsActive = 0;

                return true;
            }

            return false;
        }

        public IReadOnlyList<int> GetSwitchableIndices()
        {
            var result = new List<int>();

            for (int i = 0; i < _slots.Length; i++)
            {
                if (i == _activeIndex)
                    continue;

                PokemonState pokemon = _slots[i];

                if (pokemon == null || pokemon.IsFainted)
                    continue;

                result.Add(i);
            }

            return result;
        }

        public bool AnyPokemonKnows(string moveName)
        {
            if (string.IsNullOrWhiteSpace(moveName))
                return false;

            return _slots.Any(pokemon =>
                pokemon != null &&
                pokemon.Moves.Any(move =>
                    move is MoveState moveState &&
                    moveState.Name == moveName));
        }

        public void HealAll()
        {
            foreach (var pokemon in _slots)
            {
                if (pokemon == null)
                    continue;

                pokemon.CurrentHP = pokemon.MaxHP;
                pokemon.ClearStatus();
                pokemon.ResetStatStages();
                pokemon.VolatileStatuses.Clear();

                pokemon.LastDamageDealt = 0;
                pokemon.LastDamageTaken = 0;
                pokemon.turnsActive = 0;

                RestoreAllPP(pokemon);
            }
        }

        private static void RestoreAllPP(PokemonState pokemon)
        {
            foreach (var move in pokemon.Moves)
            {
                if (move is MoveState moveState)
                {
                    moveState.PP = moveState.MaxPP;
                }
            }
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _slots.Length;
        }

        public override string ToString()
        {
            var lines = _slots.Select((pokemon, index) =>
                $"  [{index}]{(index == _activeIndex ? "*" : " ")} {pokemon}");

            return $"Team:\n{string.Join("\n", lines)}";
        }
    }
}