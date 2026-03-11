// Design: Value Object — IVs are immutable (init-only), EVs are mutable.
// IVs: random values generated via RandomHelper if not specified.
// EVs: implements IPBEStatCollection for battle engine access.
// Layer: Model/Pkmn.
﻿using PokemonGame.Core.Model.Pkmn.Interface;
using PokemonGame.Model.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Core.Model.Pkmn
{
    internal sealed class EVs : IPBEStatCollection
    {
        public byte HP { get; set; }
        public byte Attack { get; set; }
        public byte Defense { get; set; }
        public byte SpAttack { get; set; }
        public byte SpDefense { get; set; }
        public byte Speed { get; set; }

        public EVs() { }
        public EVs(IPBEReadOnlyStatCollection other)
        {
            HP = other.HP;
            Attack = other.Attack;
            Defense = other.Defense;
            SpAttack = other.SpAttack;
            SpDefense = other.SpDefense;
            Speed = other.Speed;
        }

        public void CopyFrom(IPBEReadOnlyStatCollection other)
        {
            HP = other.HP;
            Attack = other.Attack;
            Defense = other.Defense;
            SpAttack = other.SpAttack;
            SpDefense = other.SpDefense;
            Speed = other.Speed;
        }
    }

    internal sealed class IVs : IPBEReadOnlyStatCollection
    {
        public byte HP { get; }
        public byte Attack { get; }
        public byte Defense { get; }
        public byte SpAttack { get; }
        public byte SpDefense { get; }
        public byte Speed { get; }

        public IVs(byte?[] ivs)
            : this(ivs[0], ivs[1], ivs[2], ivs[3], ivs[4], ivs[5]) { }
        public IVs(byte? hp = null, byte? attack = null, byte? defense = null, byte? spAttack = null, byte? spDefense = null, byte? speed = null)
        {
            HP = hp ?? (byte)RandomHelper.Next(0, 31);
            Attack = attack ?? (byte)RandomHelper.Next(0, 31);
            Defense = defense ?? (byte)RandomHelper.Next(0, 31);
            SpAttack = spAttack ?? (byte)RandomHelper.Next(0, 31);
            SpDefense = spDefense ?? (byte)RandomHelper.Next(0, 31);
            Speed = speed ?? (byte)RandomHelper.Next(0, 31);
        }
        public IVs(byte hp, byte attack, byte defense, byte spAttack, byte spDefense, byte speed)
        {
            HP = hp;
            Attack = attack;
            Defense = defense;
            SpAttack = spAttack;
            SpDefense = spDefense;
            Speed = speed;
        }
        public IVs(IPBEReadOnlyStatCollection other)
        {
            HP = other.HP;
            Attack = other.Attack;
            Defense = other.Defense;
            SpAttack = other.SpAttack;
            SpDefense = other.SpDefense;
            Speed = other.Speed;
        }
    }
}
