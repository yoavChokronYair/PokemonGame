// Design: Entity + Static Factory pattern.
// OOP: implements IPartyPokemon, IPBEPokemon, IPBESpeciesForm.
// Layer: Model/PokemonCreation — active party slot with full stats, HP, status, and moveset.
// Note: AbilityType enum moved to Enums/PokemonEnum/PokemonEnums.cs.
// Note: CreatePlayerOwnedMon is the static factory for player-owned Pokemon.

using PokemonGame.Core.Config;
using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Core.Model.Pkmn;
using PokemonGame.Enums;
using PokemonGame.Enums.PokemonEnum;
using PokemonGame.Interface;
using PokemonGame.Interface.Pokemon;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.PokemonCreation
{
    internal sealed class PartyPokemon : IPartyPokemon
    {
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

        public uint PID { get; private set; }
        public bool IsEgg { get; set; }

        #region PBE
        public bool PBEIgnore => IsEgg;
        bool IPBEPokemon.Pokerus => Pokerus.Exists;
        string IPBEPokemon.CaughtBall => (string)CaughtBall;
        string IPBEPokemon.Item => (string)Item;
        IPBEStatCollection IPBEPokemon.EffortValues => EffortValues;
        IPBEReadOnlyStatCollection IPBEPokemon.IndividualValues => IndividualValues;
        Moveset IPBEPokemon.Moveset => Moveset;
        Moveset IPartyPokemon.Moveset => Moveset;
        #endregion

        private PartyPokemon(PokemonData species, PokemonFormData form, byte level)
        {
            Species = species;
            Form = form;
            Level = level;
        }

        public PartyPokemon(BoxPokemon other)
        {
            PID = other.PID;
            Pokerus = new Pokerus(other.Pokerus);
            IsEgg = other.IsEgg;
            MetLocation = other.MetLocation;
            MetLevel = other.MetLevel;
            MetDate = other.MetDate;
            Species = other.Species;
            Form = other.Form;
            Nickname = other.Nickname;
            Shiny = other.Shiny;
            Level = other.Level;
            EXP = other.EXP;
            AbilType = other.AbilType;
            Ability = other.Ability;
            Gender = other.Gender;
            Nature = other.Nature;
            EffortValues = other.EffortValues;
            IndividualValues = other.IndividualValues;
            CaughtBall = other.CaughtBall;
            Friendship = other.Friendship;
            SetHPToMaxHP();
        }

        public static PartyPokemon CreatePlayerOwnedMon(PokemonData species, PokemonFormData form, byte level, BaseStatsData baseStats)
        {
            PokemonGame.Core.Model.Helper.MathHelper.RNGHelper rngHelper =
                PokemonGame.Core.Model.Helper.MathHelper.RNGHelper.GenerateRandomPokemonIdentity();
            var p = new PartyPokemon(species, form, level);
            p.PID = rngHelper.PID;
            p.SetEmptyPokerus();
            p.SetDefaultNickname();
            p.Shiny = rngHelper.IsShiny();
            var bs = baseStats;
            p.SetDefaultFriendship(bs);
            p.EXP = bs.BaseExpYield;
            p.AbilType = AbilityType.Ability1;
            p.Ability = bs.Ability1;
            p.Gender = rngHelper.GenerateGender(bs.GenderRatio);
            p.Nature = rngHelper.GenerateNature();
            p.Moveset = new Moveset();
            p.EffortValues = new EVs();
            p.IndividualValues = new IVs();
            p.CaughtBall = default;
            p.HP = (ushort)PokemonStatCalculatorHelper.CalculateHP(bs.HP, p.IndividualValues.HP, p.EffortValues.HP, p.Level);
            p.SetHPToMaxHP();
            return p;
        }

        private void SetDefaultFriendship(BaseStatsData bs) { Friendship = bs.BaseFriendship; }
        private void SetEmptyPokerus() { Pokerus = new Pokerus(true); }
        private void SetDefaultNickname() { Nickname = Species.SpeciesName; }

        public void SetHPToMaxHP() { HP = MaxHP; }

        public void HealStatus()
        {
            Status1 = default;
            SleepTurns = 0;
        }

        public void HealMoves()
        {
            for (int i = 0; i < PokemonConstants.NumMoves; i++)
                Moveset[i].SetMaxPP();
        }

        public void HealFully()
        {
            SetHPToMaxHP();
            HealStatus();
            HealMoves();
        }
    }
}
