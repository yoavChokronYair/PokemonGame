using PokemonGame.Core.Config;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Network.Packets;

namespace PokemonGame.Server.BattleRoom
{
    public static class TeamBuilder
    {
        private static ServiceFactory? _factory;

        public static void Initialize(ServiceFactory factory)
        {
            _factory = factory;
        }

        public static PokemonTeam BuildFromPlayer(int battlePlayerId, List<BattlePokemonDto> dtos)
        {
           
            if (_factory == null)
                throw new InvalidOperationException("TeamBuilder not initialized.");
            if (battlePlayerId <= 0)
                throw new InvalidOperationException($"Invalid battlePlayerId: {battlePlayerId}");
            var moveTranslator = new MoveTranslator(new MoveService(_factory));
            var abilityTranslator = new AbilityTranslator(
                new AbilityService(_factory), moveTranslator);
            var itemTranslator = new ItemTranslator(
                new ItemService(_factory), moveTranslator);
            var translator = new TeamTranslator(
                new PokemonService(_factory), moveTranslator, abilityTranslator, itemTranslator);   

            return translator.LoadTeamByID(battlePlayerId); 
            
        }

        public static PokemonTeam BuildFromDtos(List<BattlePokemonDto> dtos)
        {
            var states = new List<PokemonState>();

            foreach (var dto in dtos)
            {
                states.Add(new PokemonState
                {
                    Name = dto.Name,
                    PokedexId = dto.PokedexId,
                    Level = dto.Level,
                    CurrentHP = dto.Hp,
                    MaxHP = dto.MaxHp,
                });
            }

            while (states.Count < PokemonConstants.PartyCapacity)
                states.Add(new PokemonState { Name = "Empty", CurrentHP = 0, MaxHP = 1 });

            return PokemonTeam.Create(states);
        }
    }
}