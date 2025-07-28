using PokemonGameModel.Model.Data.MapData;


namespace PokemonGameModel.Model.Helper
{
    public class RouteEncounterHelper
    {
        private readonly Random rng = new Random();

        public List<Encounter> Encounters { get; set; }

        public RouteEncounterHelper(MapData routeDataList)
        {
            Encounters = routeDataList.Encounters;

        }

        /// <summary>
        /// Gets a random Pokémon encounter from a specific route and environment (Grass/Water/Cave).
        /// </summary>
        public Encounter GetRandomEncounter(string routeName, string environment)
        {
            

            if (Encounters == null || Encounters.Count == 0)
                return null;

            // Shuffle the encounters
            var shuffledEncounters = Encounters.OrderBy(_ => rng.Next()).ToList();

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
