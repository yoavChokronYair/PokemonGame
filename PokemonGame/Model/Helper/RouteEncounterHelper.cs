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
            {

                if (route == null || route.Encounters.Count == 0)
                    return null;

                double roll = rng.NextDouble(); // Random number between 0.0 and 1.0
                double cumulative = 0.0;

                foreach (var spawn in route.Encounters)
                {
                    cumulative += spawn.Rarity;
                    if (roll <= cumulative)
                    {
                        return spawn;
                    }
                }

                // Fallback: return last encounter if none matched due to rounding
                return route.Encounters.Last();
            }
        }
    }
}
