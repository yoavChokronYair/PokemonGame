using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public interface IStoryPlayerService
    {
        StorySaveTree LoadAll(int userId);
        void SaveAll(StorySaveTree save);
        void SetStoryPlayer(int userId);
        List<StoryPlayerSummary> GetSummaries(int userId);
    }

    public class LocalStoryPlayerService : IStoryPlayerService
    {
        private readonly TrainerInfoRepository _trainerInfoRepo;
        private readonly BadgeRepository _badgeRepo;
        private readonly StoryFlagRepository _storyFlagRepo;
        private readonly DefeatedTrainerRepository _defeatedTrainerRepo;
        private readonly ItemTakenRepository _itemTakenRepo;
        private readonly TradedPokemonRepository _tradedPokemonRepo;
        private readonly BagInventoryRepository _bagInventoryRepo;
        private readonly PokedexRepository _pokedexRepo;
        private readonly PartyRepository _partyRepo;
        private readonly StoryPlayerRepository _storyPlayerRepo;

        public LocalStoryPlayerService()
        {
            var f = ServiceFactory.Instance;
            _trainerInfoRepo = f.TrainerInfoRepository;
            _badgeRepo = f.BadgeRepository;
            _storyFlagRepo = f.StoryFlagRepository;
            _defeatedTrainerRepo = f.DefeatedTrainerRepository;
            _itemTakenRepo = f.ItemTakenRepository;
            _tradedPokemonRepo = f.TradedPokemonRepository;
            _bagInventoryRepo = f.BagInventoryRepository;
            _pokedexRepo = f.PokedexRepository;
            _partyRepo = f.PartyRepository;
            _storyPlayerRepo = f.StoryPlayerRepository;
        }

        internal LocalStoryPlayerService(
            TrainerInfoRepository trainerInfoRepo,
            BadgeRepository badgeRepo,
            StoryFlagRepository storyFlagRepo,
            DefeatedTrainerRepository defeatedTrainerRepo,
            ItemTakenRepository itemTakenRepo,
            TradedPokemonRepository tradedPokemonRepo,
            BagInventoryRepository bagInventoryRepo,
            PokedexRepository pokedexRepo,
            PartyRepository partyRepo,
            StoryPlayerRepository storyPlayerRepo)
        {
            _trainerInfoRepo = trainerInfoRepo;
            _badgeRepo = badgeRepo;
            _storyFlagRepo = storyFlagRepo;
            _defeatedTrainerRepo = defeatedTrainerRepo;
            _itemTakenRepo = itemTakenRepo;
            _tradedPokemonRepo = tradedPokemonRepo;
            _bagInventoryRepo = bagInventoryRepo;
            _pokedexRepo = pokedexRepo;
            _partyRepo = partyRepo;
            _storyPlayerRepo = storyPlayerRepo;
        }

        // ── Create ────────────────────────────────────────────────────────────

        public void SetStoryPlayer(int userId)
        {
            var player = new StoryPlayerData { UserID = userId };
            _storyPlayerRepo.Save(player);
        }

        // ── Load ──────────────────────────────────────────────────────────────

        public StorySaveTree LoadAll(int userId)
        {
            var player = _storyPlayerRepo.GetPlayerUserId(userId)
                ?? throw new InvalidOperationException(
                       $"No story player found for user {userId}.");

            int pid = player.PlayerID;

            return new StorySaveTree
            {
                CurrentPlayer = player,
                TrainerInfo = _trainerInfoRepo.Load(pid),
                Badges = _badgeRepo.LoadAll(pid),
                StoryFlags = _storyFlagRepo.LoadAll(pid),
                DefeatedTrainers = _defeatedTrainerRepo.LoadAll(pid),
                ItemsTaken = _itemTakenRepo.LoadAll(pid),
                TradedPokemon = _tradedPokemonRepo.LoadAll(pid),
                BagInventory = _bagInventoryRepo.LoadAll(pid),
                Pokedex = _pokedexRepo.LoadAll(pid),
                Party = _partyRepo.LoadAll(pid),
            };
        }

        public List<StoryPlayerSummary> GetSummaries(int userId)
        {
            var players = _storyPlayerRepo.GetPlayersUserId(userId);

            return players.Select(p =>
            {
                var trainerInfo = _trainerInfoRepo.Load(p.PlayerID);
                var badgeCount = _badgeRepo.LoadAll(p.PlayerID).Count(b => b.IsObtained == 1);

                return new StoryPlayerSummary
                {
                    PlayerID = p.PlayerID,
                    UserID = p.UserID,
                    Name = trainerInfo.Name,
                    TimePlayed = trainerInfo.TimePlayed,
                    BadgeCount = badgeCount,
                };
            }).ToList();
        }

        // ── Save ──────────────────────────────────────────────────────────────

        public void SaveAll(StorySaveTree save)
        {
            int pid = save.CurrentPlayer.PlayerID;

            _trainerInfoRepo.Save(save.TrainerInfo);
            _badgeRepo.SaveAll(save.Badges);
            _bagInventoryRepo.SaveAll(save.BagInventory);
            _pokedexRepo.SaveAll(save.Pokedex);
            _partyRepo.SaveAll(save.Party);

            foreach (var flagId in save.StoryFlags) _storyFlagRepo.Add(pid, flagId);
            foreach (var trainerId in save.DefeatedTrainers) _defeatedTrainerRepo.Add(pid, trainerId);
            foreach (var npcId in save.ItemsTaken) _itemTakenRepo.Add(pid, npcId);
            foreach (var pokedexId in save.TradedPokemon) _tradedPokemonRepo.Add(pid, pokedexId);
        }
    }

    // ── Result container ─────────────────────────────────────────────────────

    public class StorySaveTree
    {
        public StoryPlayerData CurrentPlayer { get; set; } = new();
        public TrainerInfoData TrainerInfo { get; set; } = new();
        public List<BadgeData> Badges { get; set; } = new();
        public List<int> StoryFlags { get; set; } = new();
        public List<int> DefeatedTrainers { get; set; } = new();
        public List<int> ItemsTaken { get; set; } = new();
        public List<int> TradedPokemon { get; set; } = new();
        public List<BagInventoryData> BagInventory { get; set; } = new();
        public List<PokedexData> Pokedex { get; set; } = new();
        public List<PartyData> Party { get; set; } = new();
    }

    public class StoryPlayerSummary
    {
        public int PlayerID { get; set; }
        public int UserID { get; set; }
        public string Name { get; set; } = "";
        public string TimePlayed { get; set; } = "00:00:00";
        public int BadgeCount { get; set; }
    }
}