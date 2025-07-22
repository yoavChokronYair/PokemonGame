using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Model.Helper
{
    public static class RandomHelper
    {
        private static readonly Random _rng = new Random();

        private static readonly object _lock = new object();

        public static int Next(int minValue, int maxValue)
        {
            lock (_lock)
            {
                return _rng.Next(minValue, maxValue);
            }
        }

        public static bool NextBool(double probabilityTrue = 0.5)
        {
            lock (_lock)
            {
                return _rng.NextDouble() < probabilityTrue;
            }
        }
        public static bool ShouldTriggerEncounter(double baseEncounterRate, double encounterModifier = 1.0)
        {
            baseEncounterRate = (int)(baseEncounterRate * 256.0);

            // Clamp base rate to valid range
            baseEncounterRate = Clamp(baseEncounterRate, 0, 255);

            // Apply modifier from items, abilities, etc.
            int effectiveRate = (int)(baseEncounterRate * encounterModifier);

            // Clamp effective rate to [0, 255]
            effectiveRate = Clamp(effectiveRate, 0, 255);

            // Generate a random number between 0 and 255
            int roll = Next(0, 256);

            // Encounter occurs if roll is less than the effective rate
            return roll < effectiveRate;
        }
        //this is bullshit
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
