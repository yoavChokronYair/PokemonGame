using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Data;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace PokemonGame.Model.PokemonCreation
{
    public class PlayerPokemonGeneration : IPokemon
    {
        // === Identity & Metadata ===
        public int ID { get; }
        public int PokedexID { get; set; }
        public string Species { get; private set; }
        public string Nickname { get; set; }

        // === Appearance ===
        public BitmapImage Sprite { get; set; }
        public BitmapImage Image { get; set; }
        public bool IsShiny { get; set; }
        public bool IsMale { get; set; }

        // === Typing & Traits ===
        public PokemonType[] Types { get; } = new PokemonType[2];
        public NatureType Nature { get; set; }
        public AbilityType Ability { get; set; } // Placeholder for ability implementation
        public GrowthRateType GrowthRate { get; set; } // Placeholder for enum use
        public int Friendship { get; set; } // Base 0, can be used for evolution mechanics

        // === Stats ===
        public int Level { get; set; }
        public int MaxHP { get; set; }
        public double CurrentHp { get; set; }
        public IStatValues IVs { get; private set; }
        public IStatValues EVs { get; private set; }

        // === Combat Data ===
        public Dictionary<MoveData,int> Moves { get; private set; }
        public int PokemonXP { get; set; }
        public StatusType StatusType { get; set; }
        public bool IsFainted { get; set; }

        // === Constructor ===
        public PlayerPokemonGeneration(WildPokemonGenartion wildPokemon, string nickname, double currentHP, StatusType pokemonStatus)
        {
            ID = wildPokemon.ID;
            PokedexID = wildPokemon.PokedexID;
            Species = wildPokemon.Species;
            Nickname = nickname;

            Sprite = wildPokemon.Sprite;
            Image = wildPokemon.Image;
            IsShiny = wildPokemon.IsShiny;
            IsMale = wildPokemon.IsMale;

            Types[0] = wildPokemon.Types[0];
            Types[1] = wildPokemon.Types[1];
            Nature = wildPokemon.nature;

            Ability = wildPokemon.Ability;
            GrowthRate = GrowthRateType.MediumFast; // Assumed default
            Friendship = 0; // Initial friendship level

            Level = wildPokemon.Level;
            MaxHP = wildPokemon.MaxHP;
            CurrentHp = currentHP;
            IsFainted = (currentHP <= 0);

            IVs = wildPokemon.IVs;
            EVs = wildPokemon.EVs;

            Moves = wildPokemon.Moves;
            PokemonXP = 0;
            StatusType = pokemonStatus;
        }
    }
}
