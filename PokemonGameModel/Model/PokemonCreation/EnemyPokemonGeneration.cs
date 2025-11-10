using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Helper;
using PokemonGame.Services.Data;
using PokemonGame.Services.Data.MapData;
using PokemonGame.Services.Enums.PokemonEnum;


namespace PokemonGame.Model.PokemonCreation
{
    public class EnemyPokemonGeneration : IPokemon
    {
        // Basic Info
        public string Species { get; set; }
        public int Level { get; set; }
        public int ID { get; set; }
        public int PokedexID { get; set; }

        // HP
        public int MaxHP { get; set; }
        public int CurrentHp { get; set; }

        // Stats
        public StatValues IVs { get; set; }
        public StatValues EVs { get; set; }

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
        public AbilityCategoryType Ability { get;  set; }
        public PokemonType[] Types { get; set;}
        public int CatchRate { get; set; }
        public StatValues BaseStats { get; set; }
        public GrowthRateType GrowthRate { get; set; }
        public StatusType StatusType { get; set; }

        public void GenerateWildPokemon(Encounter species, PokemonData pokemon)
        { 
            // Level and HP
            this.Level = RandomHelper.Next(species.MinLevel, species.MaxLevel);
            this.GeneratePokemonID(pokemon);
            this.GenerateIvsAndEvs(pokemon);
            this.GeneratePokemonMoves(pokemon);
            // Images
            string uri = $"pack://application:,,,/Images/GenOnePokemon/{PokedexID}.png";
            this.Sprite = uri;
            this.Image = uri;
            this.CatchRate = pokemon.CatchRate;
            this.StatusType = StatusType.None;
        }
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

    }
}
