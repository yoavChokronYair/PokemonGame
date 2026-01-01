using PokemonGame.Core.Config;
using PokemonGame.Services.Data.GameData.Move;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Core.Model.Pkmn
{
    public class Moveset
    {
        public sealed class MovesetSlot
        {
            public MoveData? Move { get; set; }
            public int PP { get; set; }
            public byte PPUps { get; set; }
            public MovesetSlot() { }
            public MovesetSlot(MovesetSlot other)
            {
                Move = other.Move;
                PPUps = other.PPUps;
                PP = other.PP;
            }
            public void Clear()
            {
                Move = default;
                PP = 0;
                PPUps = 0;
            }
            //TODO : Implement PP Ups logic
            public void SetMaxPP()
            {
                PP = Move.PPTier * 5;
            }           
        }
        private readonly MovesetSlot[] _slots;
        public MovesetSlot this[int index] => _slots[index];
        public int Count => _slots.Length;
        public Moveset()
        {
            _slots = new MovesetSlot[PokemonConstants.NumMoves];
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = new MovesetSlot();
            }
        }
        public Moveset(Moveset other)
        {
            _slots = new MovesetSlot[PokemonConstants.NumMoves];
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = new MovesetSlot(other[i]);
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

        public int GetFirstEmptySlot()
        {
            for (int i = 0; i < PokemonConstants.NumMoves; i++)
            {
                if (_slots[i].Move == default)
                {
                    return i;
                }
            }
            return -1;
        }

    }

}
