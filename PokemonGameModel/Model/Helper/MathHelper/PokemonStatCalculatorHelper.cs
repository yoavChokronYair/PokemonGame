// Design: Value Object / Calculator — computes all final stats once and caches them as properties.
// Layer: Model/Helper/MathHelper — Gen 3+ stat formula (HP and non-HP stats, nature modifier).
// CANONICAL stat calculator — PokemonDomain delegates to this class (no duplicate formulas elsewhere).
// Uses NatureHelper.GetNatureModifiers for nature modifier lookups.
﻿using PokemonGame.Constants;
using PokemonGame.Enums;
using PokemonGame.Interface;

namespace PokemonGame.Core.Model.Helper.MathHelper
{
    public class PokemonStatCalculatorHelper 
    {
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpecialAttack { get; set; }
        public int SpecialDefense { get; set; }
        public int Speed { get; set; }
        public PokemonStatCalculatorHelper(
            int baseHP, int baseAttack, int baseDefense, int baseSpecialAttack, int baseSpecialDefense, int baseSpeed,
            int ivHP, int ivAttack, int ivDefense, int ivSpecialAttack, int ivSpecialDefense, int ivSpeed,
            int evHP, int evAttack, int evDefense, int evSpecialAttack, int evSpecialDefense, int evSpeed,
            int level, NatureType nature)
        {
            ValidateIVs(ivHP, ivAttack, ivDefense, ivSpecialAttack, ivSpecialDefense, ivSpeed);
            ValidateEVs(evHP, evAttack, evDefense, evSpecialAttack, evSpecialDefense, evSpeed);

            var natureModifiers = NatureHelper.GetNatureModifiers(nature);

            this.HP = CalculateHP(baseHP, ivHP, evHP, level);
            this.Attack = CalculateStat(baseAttack, ivAttack, evAttack, level, natureModifiers.atk);
            this.Defense = CalculateStat(baseDefense, ivDefense, evDefense, level, natureModifiers.def);
            this.SpecialAttack = CalculateStat(baseSpecialAttack, ivSpecialAttack, evSpecialAttack, level, natureModifiers.spAtk);
            this.SpecialDefense = CalculateStat(baseSpecialDefense, ivSpecialDefense, evSpecialDefense, level, natureModifiers.spDef);
            this.Speed = CalculateStat(baseSpeed, ivSpeed, evSpeed, level, natureModifiers.speed);
        }
        private static void ValidateEVs(params int[] evs)
        {
            int total = evs.Sum();

            foreach (var ev in evs)
            {
                if (ev < 0 || ev > 255)
                    throw new ArgumentOutOfRangeException(nameof(evs), "Each EV must be between 0 and 255.");
            }

            if (total > 510)
                throw new ArgumentOutOfRangeException(nameof(evs), "Total EVs cannot exceed 510.");
        }
        private static void ValidateIVs(params int[] ivs)
        {
            foreach (var iv in ivs)
            {
                if (iv < 0 || iv > 31)
                    throw new ArgumentOutOfRangeException(nameof(ivs), "Each IV must be between 0 and 31.");
            }
        }
        public static int CalculateHP(int baseStat, int iv, int ev, int level)
        {
            int evContribution = ev / 4; // floor division
            return ((2 * baseStat + iv + evContribution) * level) / 100 + level + 10;
        }
        public static int CalculateStat(int baseStat, int iv, int ev, int level, double natureModifier)
        {
            int evContribution = ev / 4; // floor division
            int baseValue = ((2 * baseStat + iv + evContribution) * level) / 100 + 5;
            return (int)Math.Floor(baseValue * natureModifier);
        }
    }
}
