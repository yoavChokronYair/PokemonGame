using PokemonGame.Core.Config;
using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Core.Model.Pkmn;
using PokemonGame.Core.Model.Pkmn.Interface;
using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Services.Data.Items;
using PokemonGame.Services.Data.Pokemon;
using PokemonGame.Services.DataProvider;
using PokemonGame.Services.Enums.PokemonEnum;
using PokemonGameModel.Model.Map;
using SQLitePCL;
using System;

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
    internal sealed class PartyPokemon : IPartyPokemon
    {
        //TODO: abillity type change logic
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
        public ItemData CaughtBall { get; set; }

        public ItemData Item { get; set; }
        public AbilityType AbilType { get; private set; }
        public AbilityData Ability { get; set; }
        public NatureType Nature { get; set; }

        public ushort HP { get; set; }
        public ushort MaxHP { get; private set; }
        public StatusType Status1 { get; set; }
        public byte SleepTurns { get; set; }

        public Moveset Moveset { get; set; }

        public EVs EffortValues { get; set; }
        public IVs IndividualValues { get; set; }

        public uint PID { get; private set; } // Currently only used for Spinda spots, characteristic, and Wurmple evolution
        public bool IsEgg { get; set; }

        #region PBE
        public bool PBEIgnore => IsEgg;
        ItemData IPBEPokemon.CaughtBall => CaughtBall;
        ItemData IPBEPokemon.Item => Item;
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

        public static PartyPokemon CreatePlayerOwnedMon(PokemonData species, PokemonFormData form, byte level)
        {
            RNGHelper RNGHelper = RNGHelper.GenerateRandomPokemonIdentity();
            var p = new PartyPokemon(species, form, level)
            {
                PID = RNGHelper.PID
            };
            p.SetDefaultNickname();
            p.Shiny = RNGHelper.IsShiny();
            var bs = GameDataProvider.Instance.GetBaseStatsData(species.PokemonID);
            p.Friendship = bs.BaseFriendship;
            p.EXP = bs.BaseExpYield;
            p.AbilType = RNGHelper.GetAbilityNumber(bs);
            p.Ability = RNGHelper.GetAbility(bs,p.AbilType);
            p.Gender = RNGHelper.GenerateGender(bs.GenderRatio);
            p.Nature = RNGHelper.GenerateNature();
            p.Moveset = new Moveset();
            p.EffortValues = new EVs();
            p.IndividualValues = new IVs();
            p.CaughtBall = default;
            p.HP = (ushort)PokemonStatCalculatorHelper.CalculateHP(bs.HP, p.IndividualValues.HP, p.EffortValues.HP, p.Level);
            p.SetHPToMaxHP();
            return p;
        }
        private void SetDefaultNickname()
        {
            Nickname = Species.SpeciesName;
        }
        public void SetHPToMaxHP()
        {
            HP = MaxHP;
        }
        public void HealStatus()
        {
            Status1 = default;
            SleepTurns = 0;
        }
        public void HealMoves()
        {
            for (int i = 0; i < PokemonConstants.NumMoves; i++)
            {
                Moveset[i].SetMaxPP();
            }
        }
        public void HealFully()
        {
            SetHPToMaxHP();
            HealStatus();
            HealMoves();
        }
    }
}