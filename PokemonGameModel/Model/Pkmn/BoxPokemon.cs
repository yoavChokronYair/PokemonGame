using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Core.Model.Pkmn.Interface;
using PokemonGame.Enums;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.Services.Enums.PokemonEnum;


namespace PokemonGame.Core.Model.Pkmn
{
    internal sealed class BoxPokemon : IPBESpeciesForm
    {
        public bool IsEgg { get; set; }
        public uint PID { get; set; }
        public string MetLocation { get; set; }
        public byte MetLevel { get; set; }
        public DateTime MetDate { get; set; }

        public PokemonData Species { get; set; }
        public PokemonFormData Form { get; set; }
        public GenderType Gender { get; set; }

        public string Nickname { get; set; }
        public bool Shiny { get; set; }
        public byte Level { get; set; }
        public uint EXP { get; set; }
        public byte Friendship { get; set; }
        public string CaughtBall { get; set; }

        public Pokerus Pokerus { get; set; }
        public string Item { get; set; }
        public AbilityType AbilType { get; private set; }
        public AbilityData Ability { get; set; }
        public NatureType Nature { get; set; }

        public BoxMoveset Moveset { get; set; }
        public EVs EffortValues { get; set; }
        public IVs IndividualValues { get; set; }

        PokemonData IPBESpeciesForm.Species => throw new NotImplementedException();

        PokemonFormData IPBESpeciesForm.Form => throw new NotImplementedException();

        private BoxPokemon() { }
        public BoxPokemon(PartyPokemon other)
        {
            IsEgg = other.IsEgg;
            PID = other.PID;
            Pokerus = new Pokerus(other.Pokerus);
            MetLocation = other.MetLocation;
            MetLevel = other.MetLevel;
            MetDate = other.MetDate;
            Species = other.Species;
            Form = other.Form;
            Gender = other.Gender;
            Nickname = other.Nickname;
            Shiny = other.Shiny;
            Level = other.Level;
            EXP = other.EXP;
            Friendship = other.Friendship;
            CaughtBall = other.CaughtBall;
            Item = other.Item;
            AbilType = other.AbilType;
            Ability = other.Ability;
            Nature = other.Nature;
            Moveset = new BoxMoveset(other.Moveset);
            EffortValues = other.EffortValues;
            IndividualValues = other.IndividualValues;
        }

        public static BoxPokemon CreateDaycareEgg(PokemonData species, PokemonFormData form, GenderType gender, byte cycles, byte level, uint exp, bool shiny,
            NatureType nature, (AbilityType Type,AbilityData Abil) ability, IVs ivs, BoxMoveset moves)
        {
            RNGHelper RNGHelper = RNGHelper.GenerateRandomPokemonIdentity();
            var p = new BoxPokemon();
            p.PID = (uint)RNGHelper.PID;
            p.Pokerus = new Pokerus(true);
            p.IsEgg = true;
            p.Level = level;
            p.Nickname = "Egg";
            p.Species = species;
            p.Form = form;
            p.EffortValues = new EVs();
            p.MetLevel = level;
            p.MetDate = DateTime.Now.Date;
            p.Gender = gender;
            p.Friendship = cycles;
            p.EXP = exp;
            p.Shiny = shiny;
            p.Nature = nature;
            p.AbilType = ability.Type;
            p.Ability = ability.Abil;
            p.IndividualValues = ivs;
            p.Moveset = moves;
            return p;
        }
    }

   
}