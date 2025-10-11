using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;

namespace PokemonGame.Model.PokemonCreation
{
    //TODO: add more generations 
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
        public StatValues BaseStats { get; set; }


        public void GeneratePokemonID(PokemonData pokemon)
        {
            var randomHelper = RNGHelper.GenerateRandomPokemonIdentity();
            this.ID = randomHelper.SecretID;
            this.PokedexID = pokemon.Number;
            this.Species = pokemon.Name;
            this.Nature = randomHelper.GetNature();
            this.Ability = pokemon.Abilitys[randomHelper.GetAbilityNumber(pokemon)];
            this.Types = new PokemonType[2];
            this.Types[0] = pokemon.Type1;
            this.Types[1] = pokemon.Type2;
            this.IsMale = randomHelper.IsFemale(pokemon.MaleGenderPercent);
            this.IsShiny = randomHelper.IsShiny();
            this.MaxHP = pokemon.IVs.HP + (pokemon.IVs.HP * Level / 100);
            this.CurrentHp = MaxHP;
            this.GrowthRate = pokemon.GrowthRate;
        }
        public void GenerateIvsAndEvs(PokemonData pokemon)
        {
            this.BaseStats = pokemon.BaseStats;
            this.IVs = RNGHelper.GenerateAllIVs(pokemon);
            this.EVs = new StatValues();
        }

        public void GeneratePokemonMoves(PokemonData pokemon)
        {
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
        }
        //generations
        public void GenerateCaughtWildPokemon(IPokemon wildPokemon, string nickname, int currentHP, StatusType pokemonStatus)
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
        public void GenerateEggPokemon(PokemonData pokemon ,string nickname)
        {
            // Level and HP
            this.Level = 1;
            this.GeneratePokemonID(pokemon);
            this.GenerateIvsAndEvs(pokemon);
            this.GeneratePokemonMoves(pokemon);
            // Images
            string uri = $"pack://application:,,,/Images/GenOnePokemon/{PokedexID}.png";
            this.Sprite = uri;
            this.Image = uri;
            this.StatusType = StatusType.None;
            this.IsFainted = false;
            this.PokemonXP = 0;
            this.Friendship = 0;
        }
    }
}
