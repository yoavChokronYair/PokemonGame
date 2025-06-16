using PokemonGame.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Model.Helper
{
    public class RouteEncounterHelper
    {
        private readonly Random rng = new Random();

        public RouteDataList RouteDataList { get; set; }

        public RouteEncounterHelper(RouteDataList routeDataList)
        {
            RouteDataList = routeDataList;
        }

        /// <summary>
        /// Gets a random Pokémon encounter from a specific route and environment (Grass/Water/Cave).
        /// </summary>
        public Encounter GetRandomEncounter(string routeName, string environment)
        {
            List<RouteData> routeList = new List<RouteData>();

            if (environment.Equals("grass", StringComparison.OrdinalIgnoreCase))
            {
                routeList = RouteDataList.Grass;
            }
            else if (environment.Equals("water", StringComparison.OrdinalIgnoreCase))
            {
                routeList = RouteDataList.Water;
            }
            else if (environment.Equals("cave", StringComparison.OrdinalIgnoreCase))
            {
                routeList = RouteDataList.Cave;
            }

            var route = routeList.FirstOrDefault(r => r.Name.Equals(routeName, StringComparison.OrdinalIgnoreCase));
            if (route == null || route.Encounters.Count == 0)
                return null;

            // Shuffle the encounters
            var shuffledEncounters = route.Encounters.OrderBy(_ => rng.Next()).ToList();

            // Normalize rarities
            double totalRarity = shuffledEncounters.Sum(e => e.Rarity);
            if (totalRarity == 0)
                return null;

            double roll = rng.NextDouble();
            double cumulative = 0.0;

            foreach (var spawn in shuffledEncounters)
            {
                cumulative += spawn.Rarity / totalRarity;
                if (roll <= cumulative)
                {
                    return spawn;
                }
            }

            return shuffledEncounters.Last(); // fallback
        }
    }
}
