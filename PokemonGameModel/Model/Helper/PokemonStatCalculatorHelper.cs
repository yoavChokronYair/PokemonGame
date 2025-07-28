using System;
using PokemonGameModel.Enums;
using PokemonGameModel.Interface;
using PokemonGameModel.Model.Helper;

namespace PokemonGameModel.Model.Helper
{
    public class PokemonStatCalculatorHelper : IStatValues
    {
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpecialAttack { get; set; }
        public int SpecialDefense { get; set; }
        public int Speed { get; set; }

        public PokemonStatCalculatorHelper(int baseAttack, int baseDefense, int baseSpecialAttack, int baseSpecialDefense, int baseSpeed,
                                     int ivAttack, int ivDefense, int ivSpecialAttack, int ivSpecialDefense, int ivSpeed,
                                     int evAttack, int evDefense, int evSpecialAttack, int evSpecialDefense, int evSpeed,
                                     int level, NatureType nature)
        {
            var natureModifiers = NatureHelper.GetNatureModifiers(nature);

            this.Attack = CalculateStat(baseAttack, ivAttack, evAttack, level, natureModifiers.atk);
            this.Defense = CalculateStat(baseDefense, ivDefense, evDefense, level, natureModifiers.def);
            this.SpecialAttack = CalculateStat(baseSpecialAttack, ivSpecialAttack, evSpecialAttack, level, natureModifiers.spAtk);
            this.SpecialDefense = CalculateStat(baseSpecialDefense, ivSpecialDefense, evSpecialDefense, level, natureModifiers.spDef);
            this.Speed = CalculateStat(baseSpeed, ivSpeed, evSpeed, level, natureModifiers.speed);
        }

        public static int CalculateHP(int baseStat, int iv, int ev, int level)
        {
            return ((2 * baseStat + iv + (ev / 4)) * level) / 100 + level + 10;
        }

        public static int CalculateStat(int baseStat, int iv, int ev, int level, double natureModifier)
        {
            double stat = (((2 * baseStat + iv + (ev / 4)) * level) / 100.0 + 5) * natureModifier;
            return (int)Math.Floor(stat);
        }
    }

}
