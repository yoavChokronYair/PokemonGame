using PokemonGame.Services.ApiClients;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Handler
{
    public class LocalBattleHistoryService : IBattleHistoryService
    {
        private readonly BattleRepository _battleRepo;
        private readonly ParticipantRepository _participantRepo;
        private readonly TeamMemberRepository _teamMemberRepo;
        private readonly BattlerPokemonRepository _battlerPokemonRepo;
        private readonly PokemonRepository _pokedexRepo;
        private readonly ItemRepository _itemRepo;
        private readonly OnlinePlayerRepository _playerRepo;
        private readonly BattleTeamSnapshotRepository _snapshotRepo;
        private readonly TeamRepository _teamRepo;

        public LocalBattleHistoryService()
        {
            var f = ServiceFactory.Instance;
            _battleRepo = f.BattleRepository;
            _participantRepo = f.ParticipantRepository;
            _teamMemberRepo = f.TeamMemberRepository;
            _battlerPokemonRepo = f.BattlerPokemonRepository;
            _pokedexRepo = f.PokemonRepository;
            _itemRepo = f.ItemRepository;
            _playerRepo = f.OnlinePlayerRepository;
            _snapshotRepo = f.BattleTeamSnapshotRepository;
            _teamRepo = f.TeamRepository;
        }

        internal LocalBattleHistoryService(
            BattleRepository battleRepo, ParticipantRepository participantRepo,
            TeamMemberRepository teamMemberRepo, BattlerPokemonRepository battlerPokemonRepo,
            PokemonRepository pokedexRepo, ItemRepository itemRepo, OnlinePlayerRepository playerRepo,
            BattleTeamSnapshotRepository snapshotRepo, TeamRepository teamRepo)
        {
            _battleRepo = battleRepo;
            _participantRepo = participantRepo;
            _teamMemberRepo = teamMemberRepo;
            _battlerPokemonRepo = battlerPokemonRepo;
            _pokedexRepo = pokedexRepo;
            _itemRepo = itemRepo;
            _playerRepo = playerRepo;
            _snapshotRepo = snapshotRepo;
            _teamRepo = teamRepo;
        }

        public List<BattleTreeData> GetBattleHistoryDisplay(int battlePlayerID, string username)
        {
            var displayList = new List<BattleTreeData>();
            var records = _battleRepo.GetPlayerBattleHistory(battlePlayerID);

            foreach (var record in records)
            {
                var participants = _participantRepo.GetParticipantsForBattle(record.BattleID);
                var playerPart = participants.FirstOrDefault(p => p.BattlePlayerID == battlePlayerID);
                var opponentPart = participants.FirstOrDefault(p => p.BattlePlayerID != battlePlayerID);

                string opponentName = "Unknown Opponent";
                if (opponentPart != null)
                {
                    var oppData = _playerRepo.LoadOnlinePlayerByID(opponentPart.BattlePlayerID);
                    opponentName = oppData?.Name ?? $"Opponent #{opponentPart.BattlePlayerID}";
                }

                displayList.Add(new BattleTreeData
                {
                    BattleID = record.BattleID,
                    BattleDate = record.BattleDate,
                    PlayerName = username,
                    OpponentName = opponentName,
                    IsPlayerWinner = playerPart?.IsWinner == 1,
                    PlayerPokemon = GetFormattedPokemonList(record.BattleID, playerPart?.BattlePlayerID),
                    OpponentPokemon = GetFormattedPokemonList(record.BattleID, opponentPart?.BattlePlayerID)
                });
            }
            return displayList;
        }

        public int SaveBattleRecord() => _battleRepo.CreateBattle();

        public void SaveParticipant(BattleParticipantData participant)
        {
            _participantRepo.SaveParticipant(participant);

            // Look up the team for this battle player and snapshot it
            var team = _teamRepo.GetTeamByBattlePlayer(participant.BattlePlayerID);
            if (team != null)
                _snapshotRepo.SaveSnapshot(participant.BattleID, participant.BattlePlayerID, team.Id);
        }

        private List<BattleHistoryPokemon> GetFormattedPokemonList(int battleId, int? battlePlayerId)
        {
            if (battlePlayerId is null) return new List<BattleHistoryPokemon>();

            var results = new List<BattleHistoryPokemon>();
            var snapshots = _snapshotRepo.GetByBattleAndPlayer(battleId, battlePlayerId.Value);

            foreach (var snapshot in snapshots)
            {
                var instance = _battlerPokemonRepo.GetPokemonInstance(snapshot.PokemonID);
                if (instance is null) continue;

                var baseData = _pokedexRepo.GetPokemonById(instance.PokedexID);
                if (baseData is null) continue;

                results.Add(new BattleHistoryPokemon
                {
                    PokedexId = baseData.PokedexID,
                    Name = baseData.Name,
                    Type1 = baseData.Type1,
                    Type2 = baseData.Type2,
                    ItemName = instance.ItemID.HasValue
                                ? (_itemRepo.GetById(instance.ItemID.Value)?.Name ?? "Unknown Item")
                                : "None"
                });
            }
            return results;
        }
    }

    public class OnlineBattleHistoryService : IBattleHistoryService
    {
        private readonly LocalBattleHistoryService _local;
        private readonly IBattleHistoryApiClient _api;

        public OnlineBattleHistoryService(IBattleHistoryApiClient api)
        {
            _local = new LocalBattleHistoryService();
            _api = api;
        }

        public List<BattleTreeData> GetBattleHistoryDisplay(int battlePlayerId, string username)
        {
            var result = _api.GetBattleHistory(battlePlayerId, username);

            if (result is null)
                return _local.GetBattleHistoryDisplay(battlePlayerId, username);

            ServiceFactory.Instance.Sync?.SyncPlayerAsync(battlePlayerId).Wait();

            return _local.GetBattleHistoryDisplay(battlePlayerId, username);
        }

        public int SaveBattleRecord()
        {
            var battleId = _api.CreateBattle();

            if (battleId is null)
                return _local.SaveBattleRecord();

            return battleId.Value;
        }

        public void SaveParticipant(BattleParticipantData participant)
        {
            _api.SaveParticipant(participant);
            _local.SaveParticipant(participant);
        }
    }
}