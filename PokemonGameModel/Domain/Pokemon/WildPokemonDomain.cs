using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;

namespace PokemonGame.Model.Domain.Pokemon
{
    //TODO:Modify it to be bot level wildPokemon
    public class WildPokemonDomain
    {
        public PokemonState pokemonState;
        public (Stat stat, int amount)? EvYield { get; set; }
        public BotLevel BotLevel { get; set; }
        public int BaseExpYield { get; set; }
        public int BaseFriendshipYield { get; set; }
        public int CatchRate { get; set; }
        public GrowthRateType GrowthRate { get; set; }
        public WildPokemonDomain(EncounterDomain encounter)
        {
            RNGHelper rNGHelper = RNGHelper.GenerateRandomPokemonIdentity(PlayerDomain.Instance.trainerInfo.TrainerID);
            this.pokemonState = encounter.Pokemon;
            EvYield = encounter.evYield;
            pokemonState.Level = RandomHelper.Next(encounter.MinLevel, encounter.MaxLevel + 1);
            BaseExpYield = encounter.BaseExpYield;
            BaseFriendshipYield = encounter.BaseFriendshipYield;
            CatchRate = encounter.CatchRate;
            pokemonState.Nature = RNGHelper.GenerateNature();
            pokemonState.IsShiny = rNGHelper.IsShiny();
            pokemonState.gender = rNGHelper.IsFemale(encounter.femaleRatio);
            BotLevel = BotLevel.Easy;
            GrowthRate = encounter.GrowthRate;
        }
    }
}
