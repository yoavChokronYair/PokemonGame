using PokemonGame.Enums;
using PokemonGame.Interface;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace PokemonGame.Model.PokemonCreation
{
    public class PlayerPokemonGeneration : IPokemon
    {
        // === Identity & Metadata ===
        public int ID { get; } // Unique instance ID
        public int PokedexID { get; set; } // Pokedex number
        public string Species { get; private set; }
        public string Nickname { get; set; }
        public BitmapImage Sprite { get; set; } // Visual sprite
        public BitmapImage Image { get; set; } // Visual sprite
        public bool IsShiny { get; set; } // Shiny variant
        public bool IsMale { get; set; } // Gender
        public GrowthRateType GrowthRate { get; set; } // TODO: Change to enum
        public int Friendship { get; set; } // Friendship Level
        public AbilityType Ability { get; set; } // TODO: Replace with proper type
        // TODO: Add held item

        // === Stats ===
        public int Level { get; set; }
        public int MaxHP { get; set; }
        public IStatValues IVs { get; private set; }
        public IStatValues EVs { get; private set; }

        // === Combat ===
        public List<IMove> Moves { get; private set; }
        public int PokemonXP { get; set; }
        public StatusType StatusType { get; set; } // e.g., Burned, Paralyzed, etc.
        public bool IsFainted { get; set; }

        // === Initialization ===
        public PlayerPokemonGeneration(WildPokemonGenartion wildPokemon, string nickname, int currentHP)
        {
            ID = wildPokemon.ID;
            PokedexID = wildPokemon.PokedexID;
            Species = wildPokemon.Species;
            Nickname = nickname;
            Sprite = wildPokemon.Sprite;
            Image = wildPokemon.Image;
            IsShiny = wildPokemon.IsShiny;
            IsMale = wildPokemon.IsMale;
            GrowthRate = GrowthRateType.MediumFast; // Use enum instead of string
            Friendship = 0;
            Ability = wildPokemon.Ability;
            Level = wildPokemon.Level;
            MaxHP = wildPokemon.MaxHP;
            IVs = wildPokemon.IVs;
            EVs = wildPokemon.EVs;
            Moves = new List<IMove>(wildPokemon.Moves);
            PokemonXP = 0;
            StatusType = StatusType.None;
            IsFainted = (currentHP == 0);
        }
    }
}
