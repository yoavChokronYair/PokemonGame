using System.Collections.Generic;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class ProfileService
    {
        private readonly OnlinePlayerRepository _playerRepo;
        private readonly BattlePlayerSettingsRepository _settingsRepo;
        private readonly BattlePlayerStatsRepository _statsRepo;
        private readonly TeamRepository _teamRepo;
        private readonly TeamMemberRepository _teamMemberRepo;
        private readonly BattlerPokemonRepository _battlerPokemonRepo;
        private readonly PokemonRepository _pokedexRepo;
        public ProfileService()
        {
            // Accessing repositories via your existing ServiceFactory pattern
            var factory = ServiceFactory.Instance;
            _playerRepo = factory.OnlinePlayerRepository;
            _settingsRepo = factory.BattlePlayerSettingsRepository;
            _statsRepo = factory.BattlePlayerStatsRepository;
            _teamRepo = factory.TeamRepository;
            _teamMemberRepo = factory.TeamMemberRepository;
            _battlerPokemonRepo = factory.BattlerPokemonRepository;
            _pokedexRepo = factory.PokemonRepository;
        }

        /// <summary>
        /// Gathers all data needed for the Profile View in one call.
        /// </summary>
        public (BattlePlayerData Player, BattlePlayerStatsData Stats, BattlePlayerSettingsData Settings, List<TeamData> Teams)
            GetFullProfileData(int battlePlayerId)
        {
            var player = _playerRepo.LoadOnlinePlayerByID(battlePlayerId);
            var stats = _statsRepo.GetStats(battlePlayerId);
            var settings = _settingsRepo.GetSettings(battlePlayerId);
            var teams = _teamRepo.GetTeamsByBattlePlayer(battlePlayerId);

            return (player, stats, settings, teams);
        }

        /// <summary>
        /// Specifically fetches the favorite team details based on the stats table ID.
        /// </summary>
        public TeamData? GetFavoriteTeam(int battlePlayerId)
        {
            var stats = _statsRepo.GetStats(battlePlayerId);
            if (stats.FaveTeamID.HasValue)
            {
                return _teamRepo.GetTeamById(stats.FaveTeamID.Value);
            }
            return null;
        }

        /// <summary>
        /// Updates a single setting via the Settings Repository.
        /// </summary>
        public void UpdateSetting(int battlePlayerId, string columnName, int value)
        {
            _settingsRepo.SaveSetting(battlePlayerId, columnName, value);
        }
        public List<BattleHistoryPokemon> GetTeamFormattedList(int teamID)
        {
            var results = new List<BattleHistoryPokemon>();
            var slots = _teamMemberRepo.GetTeamMembers(teamID);

            foreach (var slot in slots)
            {
                var instance = _battlerPokemonRepo.GetPokemonInstance(slot.PokemonID);
                if (instance == null) continue;

                var baseData = _pokedexRepo.GetPokemonById(instance.PokedexID);
                if (baseData == null) continue;

                results.Add(new BattleHistoryPokemon
                {
                    PokedexId = baseData.PokedexID,
                    Name = baseData.Name,
                    // Add other properties if needed for the display format
                });
            }
            return results;
        }
        /// <summary>
        /// Sets the favorite team in the stats table.
        /// </summary>
        public void SetFavoriteTeam(int battlePlayerId, int teamId)
        {

            _statsRepo.SaveFaveTeam(battlePlayerId, teamId);
        }
    }
}