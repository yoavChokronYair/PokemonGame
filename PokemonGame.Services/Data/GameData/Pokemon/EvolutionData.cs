namespace PokemonGame.Services.Data.GameData.Pokemon
{
    public sealed class EvolutionData
    {
        public int PokemonID { get; set; }
        public ushort LevelRequired { get; set; }
        public string BabySpeciesName { get; set; }
        public string SpeciesName { get; set; }

        // DB stores Method as TEXT
        public string Method { get; set; }

        // DB stores FormID as INTEGER (FK to PokemonForm)
        public int FormID { get; set; }
    }
}