using PokemonGame.Enums;
using PokemonGame.Services.Data;
using PokemonGame.Services.Enums.PokemonEnum;
using System.Collections.Generic;

namespace PokemonGame.Interface
{
    public interface IPokemon
    {
        int ID { get; }
        int PokedexID { get; set; }
        string Species { get; set; }

        // === Appearance ===
        string Sprite { get; set; }
        string Image { get; set; }
        bool IsShiny { get; set; }
        bool IsMale { get; set; }

        // === Typing & Traits ===
        PokemonType[] Types { get; set; }
        NatureType Nature { get; set; }
        AbilityType Ability { get; set; } // Placeholder for ability implementation

        // === Stats ===
        int Level { get; set; }
        int MaxHP { get; set; }
        int CurrentHp { get; set; }
        StatValues IVs { get;  set; }
        StatValues EVs { get; set; }
        StatValues BaseStats { get; set; }
         GrowthRateType GrowthRate { get; set; } // Placeholder for enum use
        StatusType StatusType { get; set; }

        // === Combat Data ===
        Dictionary<MoveData, int> Moves { get; set; }
        void GenerateIvsAndEvs(PokemonData pokemon);
        void GeneratePokemonID(PokemonData pokemon);
        void GeneratePokemonMoves(PokemonData pokemon);
    }
}
