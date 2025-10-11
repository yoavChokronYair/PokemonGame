using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Data;
using PokemonGame.Model.Data.Player;
using PokemonGame.Model.Helper;

namespace PokemonGame.Model.PokemonCreation
{
    public class PlayerPokemonGeneration : IPokemon
    {
        // === Identity & Metadata ===
        public int ID { get; set; }
        public int PokedexID { get; set; }
        public string Species { get; set; }
        public string Nickname { get; set; }

        // === Appearance ===
        public string Sprite { get; set; }
        public string Image { get; set; }
        public bool IsShiny { get; set; }
        public bool IsMale { get; set; }

        // === Typing & Traits ===
        public PokemonType[] Types { get; set; } = new PokemonType[2];
        public NatureType Nature { get; set; }
        public AbilityType Ability { get; set; } // Placeholder for ability implementation
        public GrowthRateType GrowthRate { get; set; } // Placeholder for enum use
        public int Friendship { get; set; } // Base 0, can be used for evolution mechanics

        // === Stats ===
        public int Level { get; set; }
        public int MaxHP { get; set; }
        public int CurrentHp { get; set; }
        public StatValues IVs { get; set; }
        public StatValues EVs { get; set; }

        // === Combat Data ===
        public Dictionary<MoveData,int> Moves { get; set; }
        public int PokemonXP { get; set; }
        public StatusType StatusType { get; set; }
        public bool IsFainted { get; set; }
        public int CatchRate {  get; set; }
        public StatValues BaseStats { get; set; }

        // === Constructors ===
        //for all pokemons that are created by catching them
        public PlayerPokemonGeneration(IPokemon wildPokemon, string nickname, int currentHP, StatusType pokemonStatus)
        {
            this.ID = wildPokemon.ID;
            this.PokedexID = wildPokemon.PokedexID;
            this.Species = wildPokemon.Species;
            this.Nickname = nickname;
            
            this.Sprite = wildPokemon.Sprite;
            this.Image = wildPokemon.Image;
            this.IsShiny = wildPokemon.IsShiny;
            this.IsMale = wildPokemon.IsMale;
            
            this.Types[0] = wildPokemon.Types[0];
            this.Types[1] = wildPokemon.Types[1];
            this.Nature = wildPokemon.Nature;
            
            this.Ability = wildPokemon.Ability;
            this.GrowthRate = wildPokemon.GrowthRate; // Assumed default
            this.Friendship = 0; // Initial friendship level
            
            this.Level = wildPokemon.Level;
            this.MaxHP = wildPokemon.MaxHP;
            this.CurrentHp = currentHP;
            this.IsFainted = (currentHP <= 0);
            
            this.IVs = wildPokemon.IVs;
            this.EVs = wildPokemon.EVs;
            this.BaseStats = wildPokemon.BaseStats; 

            this.Moves = wildPokemon.Moves;
            this.PokemonXP = 0;
            this.StatusType = pokemonStatus;
        }

        //because of stealing data no need for generation for now. in future will need it 
        public void GeneratePokemonID(PokemonData pokemon)
        {
            return;
        }
        public void GeneratePokemonMoves(PokemonData pokemon)
        {
            return;
        }
        public void GenerateIvsAndEvs(PokemonData pokemon)
        {
            return;
        }
    }
}
