using PokemonGame.Enums;
using PokemonGame.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonGame.Interface
{
    public interface IPokemon
    {
         int ID { get; }
         int PokedexID { get; set; }
         string Species { get; set; }
         string Nickname { get; set; }

        // === Appearance ===
        BitmapImage Sprite { get; set; }
         BitmapImage Image { get; set; }
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

         // === Combat Data ===
         Dictionary<MoveData, int> Moves { get; set; }
         int PokemonXP { get; set; }
         StatusType StatusType { get; set; }
         bool IsFainted { get; set; }
    }
}
