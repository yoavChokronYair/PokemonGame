using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;

namespace PokemonGame.Model.PokemonCreation
{
    public class WildPokemonGenartion : IPokemon
    {
        // Basic Info
        public string Species { get; private set; }
        public string Nickname { get; set; }
        public int Level { get; set; }
        public int ID { get; set; }
        public int PokedexID { get; set; }

        // HP
        public int MaxHP { get; set; }
        public double CurrentHp { get; set; }

        // Stats
        public IStatValues IVs { get; private set; }
        public IStatValues EVs { get; private set; }

        // Moves
        public Dictionary<MoveData,int> Moves { get; private set; }

        // Gender & Shiny
        public bool IsMale { get; set; }
        public bool IsShiny { get; set; }

        // Images
        public BitmapImage Sprite { get; set; }
        public BitmapImage Image { get; set; }

        // Other Attributes
        public NatureType nature { get; set; }
        public int AbilityIndex { get; private set; }
        public AbilityType Ability { get; private set; }
        public PokemonType[] Types { get; } = new PokemonType[2];

        // Constructor
        public WildPokemonGenartion(Encounter species, PokemonData pokemon)
        {
            // Generate IDs
            var pid = RandomPokemonIDHelper.GeneratePID();
            ushort trainerID = 12345;
            ushort secretID = RandomPokemonIDHelper.GenerateRandomSID();
            var randomHelper = new RandomPokemonIDHelper(pid, trainerID, secretID);

            // Identification
            ID = secretID;
            PokedexID = pokemon.Number;
            Species = pokemon.Name;
            Nickname = Species;

            // Level and HP
            Level = RandomHelper.Next(species.MinLevel, species.MaxLevel);
            MaxHP = pokemon.HP + (pokemon.HP * Level / 100);
            CurrentHp = MaxHP;

            // Stats
            IVs = new StatValues
            {
                HP = pokemon.HP,
                Attack = pokemon.Attack,
                Defense = pokemon.Defense,
                SpecialAttack = pokemon.SpAtk,
                SpecialDefense = pokemon.SpDef,
                Speed = pokemon.Speed
            };

            EVs = new StatValues(); // default all 0

            // Moves (up to 4 learned by level)
            Moves = new Dictionary<MoveData, int>();
            int count = 0;

            for (int i = Level; i > 0 && count < 4; i--)
            {
                foreach (var moveLearn in pokemon.Moves)
                {
                    if (moveLearn.Level == i && count < 4)
                    {
                        MoveData move = moveLearn.Moves;
                        if (!Moves.ContainsKey(move))
                        {
                            Moves.Add(move, move.PP);
                            count++;
                        }
                    }
                }
            }

            // Gender & Shiny
            IsMale = randomHelper.IsMaleByFemalePercent(species.Rarity);
            IsShiny = randomHelper.IsShiny();

            // Images
            string uri = $"pack://application:,,,/Images/GenOnePokemon/{PokedexID}.png";
            Sprite = new BitmapImage(new Uri(uri));
            Image = new BitmapImage(new Uri(uri));
            
            // Abilities & Types
            AbilityIndex = randomHelper.GetAbilityNumber();
            Types[0] = pokemon.Type1;
            Types[1] = pokemon.Type2;
        }
    }
}
