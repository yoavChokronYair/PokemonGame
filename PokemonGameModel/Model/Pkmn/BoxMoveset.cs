using PokemonGame.Core.Config;
using PokemonGame.Services.Data.GameData.Move;

namespace PokemonGame.Core.Model.Pkmn
{
    public sealed class BoxMoveset
    {
        public sealed class BoxMovesetSlot
        {
            public MoveData Move { get; set; }
            public byte PPUps { get; set; }

            public BoxMovesetSlot() { }
            public BoxMovesetSlot(Moveset.MovesetSlot other)
            {
                Move = other.Move;
                PPUps = other.PPUps;
            }

            public void Clear()
            {
                Move = default;
                PPUps = 0;
            }
        }

        private readonly BoxMovesetSlot[] _slots;

        public BoxMovesetSlot this[int index] => _slots[index];
        public int Count => _slots.Length;

        public BoxMoveset()
        {
            _slots = new BoxMovesetSlot[PokemonConstants.NumMoves];
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = new BoxMovesetSlot();
            }
        }
        public BoxMoveset(Moveset other)
        {
            _slots = new BoxMovesetSlot[PokemonConstants.NumMoves];
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = new BoxMovesetSlot(other[i]);
            }
        }

        public bool Contains(MoveData move)
        {
            return IndexOf(move) != -1;
        }
        public int IndexOf(MoveData move)
        {
            for (int i = 0; i < PokemonConstants.NumMoves; i++)
            {
                if (_slots[i].Move == move)
                {
                    return i;
                }
            }
            return -1;
        }

        ///<summary>Forgets the move on top, and moves all of the others up once. The last slot will be empty</summary>
        public void ShiftMovesUp()
        {
            for (int i = 1; i < PokemonConstants.NumMoves; i++)
            {
                BoxMovesetSlot above = _slots[i - 1];
                BoxMovesetSlot below = _slots[i];
                above.Move = below.Move;
                above.PPUps = below.PPUps;
            }
            BoxMovesetSlot bottom = _slots[PokemonConstants.NumMoves - 1];
            bottom.Clear();
        }
    }
}
