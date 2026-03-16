namespace PokemonGame.Services.Data.GameData.Pokemon
{
    public class PokemonGeneral
    {
        // Identification
        public int PokedexID { get; set; }
        public string? Name { get; set; }

        // Types & Abilities
        public string? Type1 { get; set; }
        public string? Type2 { get; set; }
        public int? FirstAbilityID { get; set; }
        public int? SecondAbilityID { get; set; }
        public int? HiddenAbilityID { get; set; }

        // Catching & Breeding Stats
        public int? Catchrate { get; set; }
        public double? GenderRatio { get; set; }
        public int? BaseFriendship { get; set; }

        // Item Drops & Evolution Logic
        public string? FirstItem { get; set; }
        public string? SecondItem { get; set; }
        public int? PokemonEvoID { get; set; }
        public int? Evolevel { get; set; } // Renamed to match your SQL: Evolevel
        public string? EvoMethod { get; set; }
        public int? EvoItemID { get; set; }
    }
}