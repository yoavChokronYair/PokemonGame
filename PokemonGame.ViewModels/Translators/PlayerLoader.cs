using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.Store;

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
            var save = _playerService.LoadAll(UserStore.Instance.UserID);

            var player = PlayerDomain.Instance;

            ApplyTrainerInfo(save.TrainerInfo, player);
            ApplyMapLoc(save.TrainerInfo, player);
            ApplyProgressFlags(save, player);
            ApplyBadges(save.Badges, player);
            ApplyBagInventory(save.BagInventory, player);
            SeedFakeInventory(player); // ← add this after ApplyBagInventory
            ApplyPokedex(save.Pokedex, player);

            // CHANGED
            ApplyParty(save.Party, player);

            return player;
        }
        private static void SeedFakeInventory(PlayerDomain player)
        {
            if (player.trainerItemDomain.BagInventory.Count > 0)
                return;

            // ── TM (reusable, Gen 5+ style) ───────────────────────────────────
            var tmFlamethrower = new TmHmState(
                name: "TM35 Flamethrower",
                move: null!,          // swap in a real MoveState when available
                isHm: false,
                description: "Teaches Flamethrower to a compatible Pokémon.")
            {
                Id = 1001,
                Price = 3000,
            };

            // ── HM ────────────────────────────────────────────────────────────
            var hmSurf = new TmHmState(
                name: "HM03 Surf",
                move: null!,
                isHm: true,
                description: "Lets a Pokémon surf across water.")
            {
                Id = 1002,
                Price = 0,
            };

            // ── Poké Ball ─────────────────────────────────────────────────────
            var pokeBall = new PokeballState(
                name: "Poké Ball",
                caughtEffect: null!,
                condition: null!,
                multiplier: 1f,
                description: "A device for catching wild Pokémon.")
            {
                Id = 2001,
                Price = 200,
            };

            // ── Consumable heal item ───────────────────────────────────────────
            var potion = new itemsDomain
            {
                Id = 3001,
                Name = "Potion",
                Type = ItemType.Consumable,
                Description = "Restores 20 HP to a Pokémon.",
                UsableInBattle = true,
                UsableInField = true,
                Price = 300,
            };

            // ── Key item ──────────────────────────────────────────────────────
            var townMap = new KeyItemState(
                usageEffect: null!,
                condition: null!,
                registerable: false)
            {
                Id = 4001,
                Name = "Town Map",
                Description = "A map that shows your current location.",
                Price = 0,
            };

            player.trainerItemDomain.BagInventory[tmFlamethrower] = 1;
            player.trainerItemDomain.BagInventory[hmSurf] = 1;
            player.trainerItemDomain.BagInventory[pokeBall] = 10;
            player.trainerItemDomain.BagInventory[potion] = 5;
            player.trainerItemDomain.BagInventory[townMap] = 1;
        }
        private static void ApplyParty(
            List<StoryPlayerPokemonData> party,
            PlayerDomain player)
        {
            player.Team = new PlayerTeamDomain();

            var pokemonService = ServiceFactory.Instance.PokemonService;

            var translator = new TeamTranslator(
                pokemonService,
                new MoveTranslator(),
                new AbilityTranslator(),
                new ItemTranslator());

            foreach (var slot in party.OrderBy(p => p.Id))
            {
                // Load battler pokemon
                PokemonLoadResult? result =
                    pokemonService.LoadPokemon(slot.BattlerPokemonId);

                if (result == null)
                    continue;

                // Build real PokemonState
                PokemonState state =
                    translator.TranslateToDomain(result);

                state.CurrentHP = slot.CurrentHP;

                // Build player pokemon
                var pokemon = new PokemonPlayerDomain
                {
                    // Identity
                    PokemonUID = slot.PokemonUID,

                    PokemonState = state,

                    Nickname = slot.Nickname,

                    // OT
                    OriginalTrainerID = slot.OriginalTrainerID,
                    OriginalTrainerName = slot.OriginalTrainerName,

                    // Catch data
                    ObtainMethod = (ObtainMethodType)slot.ObtainMethod,
                    ObtainedAtRoute = slot.ObtainedAtRoute,
                    ObtainedAt = slot.ObtainedAt,
                    ObtainedAtLevel = slot.ObtainedAtLevel,
                    CaughtWithBall = (PokeBallType)slot.CaughtWithBall,
                    MetLocationText = slot.MetLocationText,

                    // Progression
                    Experience = slot.Experience,

                    GrowthRate = Enum.TryParse<GrowthRateType>(
                        slot.GrowthRate,
                        out var growth)
                            ? growth
                            : GrowthRateType.MediumFast,

                    // Battle state
                    CurrentHP = slot.CurrentHP,

                    PersistentStatus =
                        (StatusCondition)slot.StatusId,

                    Friendship = slot.Friendship,
                    Affection = slot.Affection,

                    // IVs
                    IV_HP = result.Battler.Iv_hp,
                    IV_Attack = result.Battler.Iv_atk,
                    IV_Defense = result.Battler.Iv_def,
                    IV_SpecialAttack = result.Battler.Iv_spAtk,
                    IV_SpecialDefense = result.Battler.Iv_spDef,
                    IV_Speed = result.Battler.Iv_speed,

                    // EVs
                    EV_HP = result.Battler.Ev_hp,
                    EV_Attack = result.Battler.Ev_atk,
                    EV_Defense = result.Battler.Ev_def,
                    EV_SpecialAttack = result.Battler.Ev_spAtk,
                    EV_SpecialDefense = result.Battler.Ev_spDef,
                    EV_Speed = result.Battler.Ev_speed,
                };

                // Copy moves
                if (state.Moves != null)
                {
                    for (int i = 0; i < Math.Min(state.Moves.Count, 4); i++)
                    {
                        pokemon.Moves[i] =
                            (MoveState)state.Moves[i];
                    }
                }

                player.Team.TryAdd(pokemon);
            }
        }
        // ── Save ─────────────────────────────────────────────────────────────

        public void Save(PlayerDomain player)
        {
            var save = new StorySaveTree
            {
                CurrentPlayer = ExtractCurrentPlayer(),
                TrainerInfo = ExtractTrainerInfo(player),
                Badges = ExtractBadges(player),
                StoryFlags = player.ProgressFlags.StoryFlags.ToList(),
                DefeatedTrainers = player.ProgressFlags.DefeatedTrainers.ToList(),
                ItemsTaken = player.ProgressFlags.ItemTaken.ToList(),
                TradedPokemon = player.ProgressFlags.TradedPokemon.ToList(),
                BagInventory = ExtractBagInventory(player),
                Pokedex = ExtractPokedex(player),
            };

            _playerService.SaveAll(save, UserStore.Instance.PlayerID);
        }
        private static StoryPlayerData ExtractCurrentPlayer() => new()
        {
            PlayerID = UserStore.Instance.PlayerID,
            UserID = UserStore.Instance.UserID,
        };

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
                e => (seen: e.Seen == 1, caught: e.Caught == 1, name: "test"));
        }

        // ── Extract helpers (domain → data) ──────────────────────────────────

        private static TrainerInfoData ExtractTrainerInfo(PlayerDomain player)
        {
            var info = player.trainerInfo;
            var loc = player.trainerMapLocDomain;

            return new TrainerInfoData
            {
                PlayerID = UserStore.Instance.PlayerID,
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