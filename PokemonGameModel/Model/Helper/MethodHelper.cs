using System;

namespace PokemonGame.Model.Helper
{
    //class for helper methods in general 
    public static class MethodHelper
    {
    }
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
    }
    public static class MathHelper
    {
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
            return RandomHelper.Next(minInclusive, maxExclusive);
        }

        public static T Choose<T>(T[] items)
        {
            if (items == null || items.Length == 0)
                throw new ArgumentException("Array cannot be null or empty.", nameof(items));

            return items[RandomHelper.Next(0, items.Length)];
        }
    }
    public static class ArrayHelper
    {
        public static void SetCenter2DArray<T>(T[,] array, T value)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            int centerRow = rows / 2;
            int centerCol = cols / 2;

            array[centerRow, centerCol] = value;
        }
        public static T? FindIn2DArray<T>(T[,] array, Func<T, bool> predicate) where T : class
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    T item = array[r, c];
                    if (item != null && predicate(item))
                        return item;
                }
            }

            return null; // not found
        }
        public static (int Row, int Col)? FindIn2DArrayIndex<T>(T[,] array, Func<T, bool> predicate)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (array[r, c] != null && predicate(array[r, c]!))
                        return (r, c);

            return null;
        }

    }
}
