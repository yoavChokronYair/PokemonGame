using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonGame.Model.PokemonCreation
{
    public class WildPokemonGenartion:IPokemon
    {
        //ToDo:replace encounter with basic pokemon creation class later 
        public string Species { get; private set; }
        public string Nickname { get; set; }
        public int Level { get; set; }
        public int MaxHP { get; set; }
        public IStatValues IVs { get; private set; }
        public IStatValues EVs { get; private set; }
        public List<IMove> Moves { get; private set; }
        public int ID { get; set; } // Unique PokedexID for the Pokemon instance
        public int PokedexID { get; set; } // Unique PokedexID for the Pokemon instance
        public bool IsMale { get; set; }
        public bool IsShiny { get; set; }
        public BitmapImage Sprite { get; set; } // Visual sprite
        public BitmapImage Image { get; set; } // Visual sprite
        public NatureType nature { get; set; } // Nature of the Pokemon
        public int AbilityIndex { get; private set; }//from 0 to 2, representing the ability index
        public AbilityType Ability { get; private set; }

        public WildPokemonGenartion(Encounter species,PokemonData pokemon)
        {
            var pid = RandomPokemonIDHelper.GeneratePID();
            ushort trainerID = 12345;
            ushort secretID = RandomPokemonIDHelper.GenerateRandomSID();
            RandomPokemonIDHelper randomPokemonIDHelper = new RandomPokemonIDHelper(pid,trainerID,secretID);
            ID = secretID;
            PokedexID = pokemon.Number;
            Species = pokemon.Name;
            Nickname = Species; // Default nickname is the species name
            Level = RandomHelper.Next(species.MinLevel, species.MaxLevel);
            MaxHP = pokemon.HP + (pokemon.HP * Level / 100); // Example formula for MaxHP
            IVs = new StatValues(); // Initialize IVs with default values
            EVs = new StatValues(); // Initialize EVs with default values
            Moves = new List<IMove>(); // Initialize Moves list
            IsMale = randomPokemonIDHelper.IsMaleByFemalePercent(species.Rarity);
            IsShiny = randomPokemonIDHelper.IsShiny();
            Sprite = new BitmapImage(new Uri($"pack://application:,,,/Images/GenOnePokemon/{ID}.png"));
            Image = new BitmapImage(new Uri($"pack://application:,,,/Images/GenOnePokemon/{ID}.png"));
            AbilityIndex = randomPokemonIDHelper.GetAbilityNumber(); // Get ability index based on PID
            
        }
    }
}
