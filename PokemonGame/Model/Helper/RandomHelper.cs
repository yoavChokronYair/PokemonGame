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
    }
}
