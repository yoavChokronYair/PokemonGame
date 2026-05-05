using PokemonGame.Services.ApiClients;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Data.Sync;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Handler
{
    public class LocalProfileService : IProfileService
    {
        private readonly OnlinePlayerRepository _playerRepo;
        private readonly BattlePlayerSettingsRepository _settingsRepo;
        private readonly BattlePlayerStatsRepository _statsRepo;
        private readonly TeamRepository _teamRepo;
        private readonly TeamMemberRepository _teamMemberRepo;
        private readonly BattlerPokemonRepository _battlerPokemonRepo;
        private readonly PokemonRepository _pokedexRepo;

        public LocalProfileService()
        {
            var factory = ServiceFactory.Instance;
            _playerRepo = factory.OnlinePlayerRepository;
            _settingsRepo = factory.BattlePlayerSettingsRepository;
            _statsRepo = factory.BattlePlayerStatsRepository;
            _teamRepo = factory.TeamRepository;
            _teamMemberRepo = factory.TeamMemberRepository;
            _battlerPokemonRepo = factory.BattlerPokemonRepository;
            _pokedexRepo = factory.PokemonRepository;
        }

        public ProfileDataTree GetFullProfileData(int battlePlayerId)
        {
            return new ProfileDataTree
            {
                Player = _playerRepo.LoadOnlinePlayerByID(battlePlayerId),
                Stats = _statsRepo.GetStats(battlePlayerId),
                Settings = _settingsRepo.GetSettings(battlePlayerId),
                Teams = _teamRepo.GetTeamsByBattlePlayer(battlePlayerId)
            };
        }

        public void UpdateSetting(int battlePlayerId, string columnName, int value)
        {
            _settingsRepo.SaveSetting(battlePlayerId, columnName, value);
        }

        public void SetFavoriteTeam(int battlePlayerId, int teamId)
        {
            _statsRepo.SaveFaveTeam(battlePlayerId, teamId);
        }

        public List<BattleHistoryPokemon> GetTeamFormattedList(int teamId)
        {
            var results = new List<BattleHistoryPokemon>();
            var slots = _teamMemberRepo.GetTeamMembers(teamId);

            foreach (var slot in slots)
            {
                var instance = _battlerPokemonRepo.GetPokemonInstance(slot.PokemonID);
                if (instance is null) continue;

                var baseData = _pokedexRepo.GetPokemonById(instance.PokedexID);
                if (baseData is null) continue;

                results.Add(new BattleHistoryPokemon
                {
                    PokedexId = baseData.PokedexID,
                    Name = baseData.Name
                });
            }

            return results;
        }
    }
    public class OnlineProfileService : IProfileService
    {
        private readonly LocalProfileService _local;
        private readonly IProfileApiClient _api;

        public OnlineProfileService(IProfileApiClient api)
        {
            _local = new LocalProfileService();
            _api = api;
        }

        public ProfileDataTree GetFullProfileData(int battlePlayerId)
        {
            var dto = _api.GetFullProfile(battlePlayerId);

            if (dto is null)
                return _local.GetFullProfileData(battlePlayerId);

            ServiceFactory.Instance.Sync?.SyncPlayerAsync(battlePlayerId).Wait();

            return _local.GetFullProfileData(battlePlayerId);
        }

        public void UpdateSetting(int battlePlayerId, string columnName, int value)
        {
            _api.UpdateSetting(battlePlayerId, columnName, value);
            _local.UpdateSetting(battlePlayerId, columnName, value);
        }

        public void SetFavoriteTeam(int battlePlayerId, int teamId)
        {
            _api.SetFavoriteTeam(battlePlayerId, teamId);
            _local.SetFavoriteTeam(battlePlayerId, teamId);
        }

        public List<BattleHistoryPokemon> GetTeamFormattedList(int teamId)
        {
            return _local.GetTeamFormattedList(teamId);
        }
    }
}