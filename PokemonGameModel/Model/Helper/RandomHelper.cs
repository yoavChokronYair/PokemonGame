using System;

namespace PokemonGameModel.Model.Helper
{
    public static class RandomHelper
    {
        private static readonly Random _rng = new Random();
        private static readonly object _lock = new object();

        // ----------------------------
        // Basic RNG
        // ----------------------------
        public static int Next(int minValue, int maxValue)
        {
            lock (_lock)
            {
                return _rng.Next(minValue, maxValue);
            }
        }

        public static double NextDouble()
        {
            lock (_lock)
            {
                return _rng.NextDouble();
            }
        }

        public static bool NextBool(double probabilityTrue = 0.5)
        {
            lock (_lock)
            {
                return _rng.NextDouble() < probabilityTrue;
            }
        }

        public static void NextBytes(byte[] buffer)
        {
            lock (_lock)
            {
                _rng.NextBytes(buffer);
            }
        }

        // ----------------------------
        // Utility Math
        // ----------------------------
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

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Range(int minInclusive, int maxExclusive)
        {
            return Next(minInclusive, maxExclusive);
        }

        public static T Choose<T>(T[] items)
        {
            if (items == null || items.Length == 0)
                throw new ArgumentException("Array cannot be null or empty.", nameof(items));

            return items[Next(0, items.Length)];
        }
    }
}
