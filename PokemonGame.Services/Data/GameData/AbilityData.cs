namespace PokemonGame.Services.Data.GameData
{
    public sealed class AbilityData
    {
        public int abilityID { get; set; }
        public string abilityName { get; set; }
        public string abilityDescription { get; set; }
        public int category { get; set; }//should be enum in model

    }
}
