namespace PokemonGame.Services.Data.GameData.Pokemon
{
    public class PokemonGeneral
    {
        // Primary Key
        public int PokedexID { get; set; }

        // Basic Info
        public string Name { get; set; }
        public string Type1 { get; set; }
        public string? Type2 { get; set; } // Nullable because some Pokemon have only one type

        // Ability References
        public int? FirstAbilityID { get; set; }
        public int? SecondAbilityID { get; set; }
        public int? HiddenAbilityID { get; set; }

        // Stats & Breeding Info
        public int? Catchrate { get; set; }
        public double? GenderRatio { get; set; }
        public int? BaseFriendship { get; set; }

        // Items & Evolution
        public string? FirstItem { get; set; }
        public string? SecondItem { get; set; }
        public int? PokemonEvoID { get; set; }
        public int? EvoLevel { get; set; }
        public string? EvoMethod { get; set; } // Default 'Level'
        public int? EvoItemID { get; set; }
    }
}