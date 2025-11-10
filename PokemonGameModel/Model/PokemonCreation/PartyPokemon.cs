using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Services.Data;
using PokemonGame.Services.Data.Pokemon;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.PokemonCreation
{
    //TODO: add more generations 
    public enum AbilityType : byte
    {
        Ability1,
        Ability2,
        AbilityH,
        NonStandard
    }
    public class PartyPokemon : IPokemon
    { //TODO: use more enums instead of basic types where possible
        public OTInfo OT { get; set; }
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
        /// <summary>Remaining egg cycles if <see cref="IsEgg"/> is true.</summary>
        public byte Friendship { get; set; }
        public string CaughtBall { get; set; }

        public string Item { get; set; }
        public AbilityType AbilType { get; private set; }
        public AbilityData Ability { get; set; }
        public NatureType Nature { get; set; }

        public ushort HP { get; set; }
        public ushort MaxHP { get; private set; }
        public StatusType Status1 { get; set; }
        public byte SleepTurns { get; set; }
        public Pokerus Pokerus { get; set; }

        public Moveset Moveset { get; set; }

        public EVs EffortValues { get; set; }
        public IVs IndividualValues { get; set; }

        public uint PID { get; private set; } // Currently only used for Spinda spots, characteristic, and Wurmple evolution
        public bool IsEgg { get; set; }
        public bool Ignore => IsEgg;
       

    }
}
