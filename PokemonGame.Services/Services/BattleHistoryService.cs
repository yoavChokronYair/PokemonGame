using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Data.Repositories.PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class BattleHistoryService
    {
        private readonly BattleRepository _battleRepo;
        private readonly ParticipantRepository _participantRepo;
        private readonly TeamMemberRepository _teamMemberRepo;
        private readonly BattlerPokemonRepository _battlerPokemonRepo;
        private readonly PokemonRepository _pokedexRepo;
        private readonly ItemRepository _itemRepo;
        private readonly OnlinePlayerRepository _playerRepo;

        public BattleHistoryService()
        {
            var f = ServiceFactory.Instance;
            _battleRepo = f.BattleRepository;
            _participantRepo = f.ParticipantRepository;
            _teamMemberRepo = f.TeamMemberRepository;
            _battlerPokemonRepo = f.BattlerPokemonRepository;
            _pokedexRepo = f.PokemonRepository;
            _itemRepo = f.ItemRepository;
            _playerRepo = f.OnlinePlayerRepository;
        }
        internal BattleHistoryService(
            BattleRepository battleRepo, ParticipantRepository participantRepo,
            TeamMemberRepository teamMemberRepo, BattlerPokemonRepository battlerPokemonRepo,
            PokemonRepository pokedexRepo, ItemRepository itemRepo, OnlinePlayerRepository playerRepo)
        {
            _battleRepo = battleRepo;
            _participantRepo = participantRepo;
            _teamMemberRepo = teamMemberRepo;
            _battlerPokemonRepo = battlerPokemonRepo;
            _pokedexRepo = pokedexRepo;
            _itemRepo = itemRepo;
            _playerRepo = playerRepo;
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

                // Fetch actual name from the DB instead of using a placeholder
                string opponentName = "Unknown Opponent";
                if (opponentPart != null)
                {
                    // Use your PlayerRepository here
                    var oppData = _playerRepo.LoadOnlinePlayerByID(opponentPart.BattlePlayerID);
                    opponentName = oppData?.Name ?? $"Opponent #{opponentPart.BattlePlayerID}";
                }

                displayList.Add(new BattleTreeData
                {
                    BattleID = record.BattleID,
                    BattleDate = record.BattleDate,
                    PlayerName = username,
                    OpponentName = opponentName, // Now shows the actual name
                    IsPlayerWinner = playerPart?.IsWinner == 1,
                    PlayerPokemon = GetFormattedPokemonList(playerPart?.TeamID),
                    OpponentPokemon = GetFormattedPokemonList(opponentPart?.TeamID)
                });
            }
            return displayList;
        }

        private List<BattleHistoryPokemon> GetFormattedPokemonList(int? teamID)
        {
            if (teamID == null) return new List<BattleHistoryPokemon>();

            var results = new List<BattleHistoryPokemon>();
            var slots = _teamMemberRepo.GetTeamMembers(teamID.Value);

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
                    Type1 = baseData.Type1,
                    Type2 = baseData.Type2,
                    ItemName = instance.ItemID.HasValue ?
                               (_itemRepo.GetById(instance.ItemID.Value)?.Name ?? "Unknown Item")
                               : "None"
                });
            }
            return results;
        }
        public int SaveBattleRecord()
        {
            return _battleRepo.CreateBattle();
        }
        public void SaveParticipant(BattleParticipantData participant)
        {
            _participantRepo.SaveParticipant(participant);
        }
    }
}