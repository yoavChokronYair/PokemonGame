using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Handler
{
    public interface IStoryPlayerService
    {
        StorySaveTree LoadAll();
        void SaveAll(StorySaveTree save);
        void SetStoryPlayer(int userId);
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

        // ── Load ─────────────────────────────────────────────────────────────
        public void SetStoryPlayer(int userId)
        {
            var player = new StoryPlayerData { UserID = userId };
            _storyPlayerRepo.Save(player);
        }

        public StorySaveTree LoadAll() => new StorySaveTree
        {
            TrainerInfo = _trainerInfoRepo.Load(),
            Badges = _badgeRepo.LoadAll(),
            StoryFlags = _storyFlagRepo.LoadAll(),
            DefeatedTrainers = _defeatedTrainerRepo.LoadAll(),
            ItemsTaken = _itemTakenRepo.LoadAll(),
            TradedPokemon = _tradedPokemonRepo.LoadAll(),
            BagInventory = _bagInventoryRepo.LoadAll(),
            Pokedex = _pokedexRepo.LoadAll(),
            Party = _partyRepo.LoadAll(),
            Players = _storyPlayerRepo.LoadAll(),
        };

        // ── Save (called on start and on every button press) ──────────────────

        public void SaveAll(StorySaveTree save)
        {
            _trainerInfoRepo.Save(save.TrainerInfo);
            _badgeRepo.SaveAll(save.Badges);
            _bagInventoryRepo.SaveAll(save.BagInventory);
            _pokedexRepo.SaveAll(save.Pokedex);
            _partyRepo.SaveAll(save.Party);
            _storyPlayerRepo.SaveAll(save.Players);

            foreach (var flagId in save.StoryFlags)
                _storyFlagRepo.Add(flagId);

            foreach (var trainerId in save.DefeatedTrainers)
                _defeatedTrainerRepo.Add(trainerId);

            foreach (var npcId in save.ItemsTaken)
                _itemTakenRepo.Add(npcId);

            foreach (var pokedexId in save.TradedPokemon)
                _tradedPokemonRepo.Add(pokedexId);
        }
    }

    // ── Result container returned to the caller ───────────────────────────────

    public class StorySaveTree
    {
        public TrainerInfoData TrainerInfo { get; set; } = new();
        public List<BadgeData> Badges { get; set; } = new();
        public List<int> StoryFlags { get; set; } = new();
        public List<int> DefeatedTrainers { get; set; } = new();
        public List<int> ItemsTaken { get; set; } = new();
        public List<int> TradedPokemon { get; set; } = new();
        public List<BagInventoryData> BagInventory { get; set; } = new();
        public List<PokedexData> Pokedex { get; set; } = new();
        public List<PartyData> Party { get; set; } = new();
        public List<StoryPlayerData> Players { get; set; } = new();
    }
}