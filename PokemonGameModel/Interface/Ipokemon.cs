using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data;
using System.Collections.Generic;

namespace PokemonGameModel.Interface
{
    public interface IPokemon
    {
        int ID { get; }
        int PokedexID { get; set; }
        string Species { get; set; }
        string Nickname { get; set; }

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
        IStatValues IVs { get;  set; }
        IStatValues EVs { get; set; }
        int CatchRate { get; set; }

        // === Combat Data ===
        Dictionary<MoveData, int> Moves { get; set; }
        StatusType StatusType { get; set; }
        bool IsFainted { get; set; }
    }
}
