using PokemonGame.Model.Helper;
using PokemonGame.Model.PokemonCreation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PokemonGame.Core.Model.Pkmn
{
    public sealed class Pokerus
    {
        private byte _b;
        public byte Strain
        {
            get => (byte)(_b >> 4);
            set => _b = (byte)((_b & 0xF) | (value << 4));
        }
        public byte DaysRemaining
        {
            get => (byte)(_b & 0xF);
            set => _b = (byte)((_b & ~0xF) | value);
        }
        public bool Exists => Strain != 0;
        public bool IsCured => Strain != 0 && DaysRemaining == 0;
        public bool IsInfected => Strain != 0 && DaysRemaining > 0;

        // Create
        public Pokerus(bool empty)
        {
            if (!empty)
            {
                CreateRandomStrain();
            }
        }
        // Clone
        public Pokerus(Pokerus other)
        {
            _b = other._b;
        }

        private void CreateRandomStrain()
        {
            Strain = (byte)RandomHelper.Next(1, 15);
            DaysRemaining = GetInitialDaysRemaining(Strain);
        }
        private void SpreadStrain(byte strain)
        {
            Strain = strain;
            DaysRemaining = GetInitialDaysRemaining(strain);
        }

        public static byte GetInitialDaysRemaining(byte strain)
        {
            return (byte)((strain % 4) + 1);
        }
    }
}
