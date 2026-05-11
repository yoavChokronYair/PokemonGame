using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Handler;

namespace PokemonGame.ViewModels.Translators
{
    public sealed class PlayerLoader
    {
        private readonly IStoryPlayerService _playerService;
        private readonly MapLoader _mapLoader;

        public PlayerLoader(IStoryPlayerService playerService, MapLoader mapLoader)
        {
            _playerService = playerService;
            _mapLoader = mapLoader;
        }

        // ── Load ─────────────────────────────────────────────────────────────

        public PlayerDomain Load()
        {
            var save = _playerService.LoadAll();
            var player = PlayerDomain.Instance;

            ApplyTrainerInfo(save.TrainerInfo, player);
            ApplyMapLoc(save.TrainerInfo, player);
            ApplyProgressFlags(save, player);
            ApplyBadges(save.Badges, player);
            ApplyBagInventory(save.BagInventory, player);
            ApplyPokedex(save.Pokedex, player);

            return player;
        }

        // ── Save ─────────────────────────────────────────────────────────────

        public void Save(PlayerDomain player)
        {
            var save = new StorySaveTree
            {
                TrainerInfo = ExtractTrainerInfo(player),
                Badges = ExtractBadges(player),
                StoryFlags = player.ProgressFlags.StoryFlags.ToList(),
                DefeatedTrainers = player.ProgressFlags.DefeatedTrainers.ToList(),
                ItemsTaken = player.ProgressFlags.ItemTaken.ToList(),
                TradedPokemon = player.ProgressFlags.TradedPokemon.ToList(),
                BagInventory = ExtractBagInventory(player),
                Pokedex = ExtractPokedex(player),
            };

            _playerService.SaveAll(save);
        }

        // ── Apply helpers (data → domain) ─────────────────────────────────────

        private static void ApplyTrainerInfo(TrainerInfoData d, PlayerDomain player)
        {
            player.trainerInfo = new TrainerInfoDomain
            {
                TrainerID = d.TrainerID,
                Name = d.Name,
                Money = d.Money,
                TimePlayed = DateTime.Today + TimeSpan.Parse(d.TimePlayed),
                Gender = (Gender)d.Gender,
                HallOfFameDebut = d.HallOfFameDebut,
            };
        }

        private void ApplyMapLoc(TrainerInfoData d, PlayerDomain player)
        {
            player.trainerMapLocDomain = new TrainerMapLocDomain
            {
                FacingDirection = (FacingDirection)d.FacingDirection,
                CurrentMap = _mapLoader.Load(d.CurrentMap),
                LastMapVisited = _mapLoader.Load(d.LastMapVisited),
                playerLoc = (d.PlayerLocX, d.PlayerLocY),
                IsSurfing = d.IsSurfing == 1,
            };

            player.trainerItemDomain.HasRunningShoes = d.HasRunningShoes == 1;
        }

        private static void ApplyProgressFlags(StorySaveTree save, PlayerDomain player)
        {
            player.ProgressFlags.StoryFlags = new HashSet<int>(save.StoryFlags);
            player.ProgressFlags.DefeatedTrainers = new HashSet<int>(save.DefeatedTrainers);
            player.ProgressFlags.ItemTaken = new HashSet<int>(save.ItemsTaken);
            player.ProgressFlags.TradedPokemon = new HashSet<int>(save.TradedPokemon);
        }

        private static void ApplyBadges(List<BadgeData> badges, PlayerDomain player)
        {
            player.Badges = badges.Select(b => new BadgeDomain
            {
                Id = b.Id,
                IsObtained = b.IsObtained == 1,
            }).ToList();
        }

        private static void ApplyBagInventory(List<BagInventoryData> items, PlayerDomain player)
        {
            // itemsDomain lookup is left to the caller — we store by ItemId key
            // and let existing bag logic resolve the domain object
            foreach (var item in items)
                player.trainerItemDomain.BagInventory
                    .Where(kv => kv.Key.Id == item.ItemId)
                    .ToList()
                    .ForEach(kv => player.trainerItemDomain.BagInventory[kv.Key] = item.Quantity);
        }

        private static void ApplyPokedex(List<PokedexData> entries, PlayerDomain player)
        {
            player.Pokedex = entries.ToDictionary(
                e => e.PokedexId,
                e => (seen: e.Seen == 1, caught: e.Caught == 1));
        }

        // ── Extract helpers (domain → data) ──────────────────────────────────

        private static TrainerInfoData ExtractTrainerInfo(PlayerDomain player)
        {
            var info = player.trainerInfo;
            var loc = player.trainerMapLocDomain;

            return new TrainerInfoData
            {
                Id = 1,
                TrainerID = info.TrainerID,
                Name = info.Name,
                Money = info.Money,
                TimePlayed = info.TimePlayed.TimeOfDay.ToString(@"hh\:mm\:ss"),
                Gender = (int)info.Gender,
                HallOfFameDebut = info.HallOfFameDebut,
                FacingDirection = (int)loc.FacingDirection,
                CurrentMap = loc.CurrentMap?.Name ?? "",
                LastMapVisited = loc.LastMapVisited?.Name ?? "",
                PlayerLocX = loc.playerLoc.x,
                PlayerLocY = loc.playerLoc.y,
                IsSurfing = loc.IsSurfing ? 1 : 0,
                HasRunningShoes = player.trainerItemDomain.HasRunningShoes ? 1 : 0,
            };
        }

        private static List<BadgeData> ExtractBadges(PlayerDomain player) =>
            player.Badges.Select(b => new BadgeData
            {
                Id = b.Id,
                IsObtained = b.IsObtained ? 1 : 0,
            }).ToList();

        private static List<BagInventoryData> ExtractBagInventory(PlayerDomain player) =>
            player.trainerItemDomain.BagInventory
                .Select(kv => new BagInventoryData
                {
                    ItemId = kv.Key.Id,
                    Quantity = kv.Value,
                })
                .ToList();

        private static List<PokedexData> ExtractPokedex(PlayerDomain player) =>
            player.Pokedex.Select(kv => new PokedexData
            {
                PokedexId = kv.Key,
                Seen = kv.Value.seen ? 1 : 0,
                Caught = kv.Value.caught ? 1 : 0,
            }).ToList();
    }
}