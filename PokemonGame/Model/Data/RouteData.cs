using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Model.Data
{
    public class Encounter
    {
        public string Pokemon { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public double Rarity { get; set; } // e.g., 0.5 for 50%
    }

    public class RouteData
    {
        public string Name { get; set; } // e.g., "Route 1"
        public List<Encounter> Encounters { get; set; } = new List<Encounter>();
    }

    public class RouteDataList
    {
        public List<RouteData> Grass { get; set; } = new List<RouteData>();
        public List<RouteData> Water { get; set; } = new List<RouteData>();
        public List<RouteData> Cave { get; set; } = new List<RouteData>();
        // Add more if needed (e.g., Fishing, RockSmash, etc.)

        public List<RouteData> AllRoutes
        {
            get
            {
                var all = new List<RouteData>();
                all.AddRange(Grass);
                all.AddRange(Water);
                all.AddRange(Cave);
                return all;
            }
        }
    }
}
