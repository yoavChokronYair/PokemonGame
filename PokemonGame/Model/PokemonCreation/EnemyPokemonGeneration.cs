using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Manager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Model.PokemonCreation
{
    public class EnemyPokemonGeneration : IPokemon
    {
        // Basic Info
        public string Species { get; set; }
        public string Nickname { get; set; }
        public int Level { get; set; }
        public int ID { get; set; }
        public int PokedexID { get; set; }

        // HP
        public int MaxHP { get; set; }
        public int CurrentHp { get; set; }

        // Stats
        public IStatValues IVs { get; set; }
        public IStatValues EVs { get; set; }

        // Moves
        public Dictionary<MoveData,int> Moves { get; set; }

        // Gender & Shiny
        public bool IsMale { get; set; }
        public bool IsShiny { get; set; }

        // Images
        public string Sprite { get; set; }
        public string Image { get; set; }


        // Other Attributes
        public NatureType Nature { get; set; }
        public int AbilityIndex { get; set; }
        public AbilityType Ability { get;  set; }
        public PokemonType[] Types { get; set;}
        public bool IsFainted { get; set; }
        public StatusType StatusType { get; set; }
        public int CatchRate { get; set; }


        // Constructors
        //constractor to create enemy pokemon from the wild
        public EnemyPokemonGeneration(Encounter species, PokemonData pokemon)
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
            Sprite = uri;
            Image = uri;
            
            // Abilities & Types
            AbilityIndex = randomHelper.GetAbilityNumber();
            Types = new PokemonType[2];
            Types[0] = pokemon.Type1;
            Types[1] = pokemon.Type2;
            IsFainted = false;
            StatusType = StatusType.None;
            this.CatchRate = pokemon.CatchRate;
        }
        public EnemyPokemonGeneration(
    int pokedexID,
    string pokemonName,
    int level,
    int currentHP,
    int maxHP,
    bool isShiny,
    bool isMale,
    NatureType nature,
    AbilityType ability,
    PokemonType[] types,
    IStatValues ivs,
    IStatValues evs,
    List<string> moveNames,
    StatusType status,
    string spriteFileName,
    string imageFileName
)
        {
            ID = RandomPokemonIDHelper.GenerateRandomSID();
            PokedexID = pokedexID;
            Species = pokemonName;
            Nickname = Species;
            Level = level;
            CurrentHp = currentHP;
            MaxHP = maxHP;
            IsShiny = isShiny;
            IsMale = isMale;
            Nature = nature;
            Ability = ability;
            Types = types;
            IVs = ivs;
            EVs = evs;
            StatusType = status;
            IsFainted = false;

            // Image URIs
            Sprite = $"pack://application:,,,/Images/GenOnePokemon/{spriteFileName}";
            Image = $"pack://application:,,,/Images/GenOnePokemon/{imageFileName}";

            // Generate Moves dictionary
            Moves = new Dictionary<MoveData, int>();
            foreach (string moveName in moveNames)
            {
                MoveData move = GameDataManager.Instance.MoveData.Moves.FirstOrDefault(m => m.ename.Equals(moveName, StringComparison.OrdinalIgnoreCase));
                if (move != null && !Moves.ContainsKey(move))
                {
                    Moves.Add(move, move.PP);
                }
            }

            // Retrieve catch rate if needed
            var pokemonData = GameDataManager.Instance.PokemonData.AllPokemons.FirstOrDefault(p => p.Number == pokedexID);
            CatchRate = pokemonData?.CatchRate ?? 150;
        }

    }
}
