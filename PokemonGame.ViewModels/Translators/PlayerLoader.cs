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

        // ─────────────────────────────────────────────────────────────
        // LOAD
        // ─────────────────────────────────────────────────────────────

        public PlayerDomain Load()
        {
            var save = _playerService.LoadAll(UserStore.Instance.UserID);

            var player = PlayerDomain.Instance;
                
            ApplyTrainerInfo(player, save.TrainerInfo);
            ApplyMapLoc(save.TrainerInfo, player);
            ApplyProgressFlags(save, player);
            ApplyBadges(save.Badges, player);
            ApplyBagInventory(save.BagInventory, player);
            ApplyPokedex(save.Pokedex, player);
            ApplyParty(save.Party, player);

            return player;
        }

        // ─────────────────────────────────────────────────────────────
        // SAVE
        // ─────────────────────────────────────────────────────────────

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

        // ─────────────────────────────────────────────────────────────
        // APPLY HELPERS: DATA → DOMAIN
        // ─────────────────────────────────────────────────────────────

        private void ApplyTrainerInfo(PlayerDomain player, TrainerInfoData data)
        {
            player.trainerInfo.TrainerID = data.TrainerID;
            player.trainerInfo.Name = data.Name;
            player.trainerInfo.Money = data.Money;
            player.trainerInfo.Gender = (Gender)data.Gender;
            player.trainerInfo.HallOfFameDebut = data.HallOfFameDebut;

            if (TimeSpan.TryParse(data.TimePlayed, out var ts))
                player.trainerInfo.TimePlayed = DateTime.Today.Add(ts);

            player.trainerMapLocDomain.FacingDirection = (FacingDirection)data.FacingDirection;
            player.trainerMapLocDomain.playerLoc = (data.PlayerLocX, data.PlayerLocY);
            player.trainerMapLocDomain.IsSurfing = data.IsSurfing == 1;

            if (!string.IsNullOrEmpty(data.CurrentMap))
                player.trainerMapLocDomain.CurrentMap = _mapLoader.Load(data.CurrentMap);

            if (!string.IsNullOrEmpty(data.LastMapVisited))
                player.trainerMapLocDomain.LastMapVisited = _mapLoader.Load(data.LastMapVisited);

            player.trainerItemDomain.HasRunningShoes = data.HasRunningShoes == 1;
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
            player.ProgressFlags.StoryFlags =
                new HashSet<int>(save.StoryFlags);

            player.ProgressFlags.DefeatedTrainers =
                new HashSet<int>(save.DefeatedTrainers);

            player.ProgressFlags.ItemTaken =
                new HashSet<int>(save.ItemsTaken);

            player.ProgressFlags.TradedPokemon =
                new HashSet<int>(save.TradedPokemon);
        }

        private static void ApplyBadges(List<BadgeData> badges, PlayerDomain player)
        {
            player.Badges = badges
                .Select(b => new BadgeDomain
                {
                    Id = b.Id,
                    IsObtained = b.IsObtained == 1,
                })
                .ToList();
        }

        // ─────────────────────────────────────────────────────────────
        // BAG INVENTORY — REAL ITEM SYSTEM
        // ─────────────────────────────────────────────────────────────
        //
        // DB table:
        // BagInventory = PlayerID + ItemId + Quantity
        //
        // Correct flow:
        // ItemId → ItemTranslator.TranslateById()
        //        → ItemService.GetItemById()
        //        → ItemTree
        //        → real domain item:
        //          PokeballState / TmHmState / KeyItemState / itemsDomain
        //
        // No fake inventory.
        // No hardcoded item IDs.
        // No BuildKnownItemDomain.
        // ─────────────────────────────────────────────────────────────

        private static void ApplyBagInventory(
            List<BagInventoryData> items,
            PlayerDomain player)
        {
            player.trainerItemDomain.BagInventory.Clear();

            var itemTranslator = new ItemTranslator(
                ServiceFactory.Instance.ItemService,
                new MoveTranslator(ServiceFactory.Instance.MoveService));

            foreach (var row in items)
            {
                if (row.Quantity <= 0)
                    continue;

                try
                {
                    itemsDomain item =
                        itemTranslator.TranslateById(row.ItemId);

                    player.trainerItemDomain.BagInventory[item] =
                        row.Quantity;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[PlayerLoader] Failed to load item ItemId={row.ItemId}, Quantity={row.Quantity}. Error: {ex.Message}");
                }
            }
        }

        private static void ApplyPokedex(List<PokedexData> entries, PlayerDomain player)
        {
            var pokemonService = ServiceFactory.Instance.PokemonService;

            player.Pokedex = entries.ToDictionary(
                e => e.PokedexId,
                e =>
                {
                    string name = $"#{e.PokedexId}";

                    try
                    {
                        var pokemon = pokemonService.LoadPokemon(e.PokedexId);

                        if (pokemon?.Battler?.Name != null)
                            name = pokemon.Battler.Name;
                    }
                    catch
                    {
                        // Keep fallback name.
                    }

                    return (
                        seen: e.Seen == 1,
                        caught: e.Caught == 1,
                        name: name
                    );
                });
        }

        private static void ApplyParty(
            List<StoryPlayerPokemonData> party,
            PlayerDomain player)
        {
            player.Team = new PlayerTeamDomain();

            var pokemonService = ServiceFactory.Instance.PokemonService;

            var translator = new TeamTranslator(
                pokemonService,
                new MoveTranslator(ServiceFactory.Instance.MoveService),
                new AbilityTranslator(ServiceFactory.Instance.AbilityService),
                new ItemTranslator(
                    ServiceFactory.Instance.ItemService,
                    new MoveTranslator(ServiceFactory.Instance.MoveService)));

            foreach (var slot in party)
            {
                PokemonLoadResult? result =
                    pokemonService.LoadPokemon(slot.BattlerPokemonId);

                if (result == null)
                    continue;

                PokemonState state =
                    translator.TranslateToDomain(result);

                state.CurrentHP = slot.CurrentHP;
                if (Enum.IsDefined(typeof(StatusCondition), slot.StatusId))
                {
                    var savedStatus = (StatusCondition)slot.StatusId;

                    if (savedStatus != StatusCondition.None)
                        state.ApplyStatus(savedStatus);
                }
                var pokemon = new PokemonPlayerDomain
                {
                    // Identity
                    PokemonUID = slot.PokemonUID,
                    PokemonState = state,
                    Nickname = slot.Nickname,

                    // Original trainer
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
                    PersistentStatus = (StatusCondition)slot.StatusId,

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

        // ─────────────────────────────────────────────────────────────
        // EXTRACT HELPERS: DOMAIN → DATA
        // ─────────────────────────────────────────────────────────────

        private static StoryPlayerData ExtractCurrentPlayer()
        {
            return new StoryPlayerData
            {
                PlayerID = UserStore.Instance.PlayerID,
                UserID = UserStore.Instance.UserID,
            };
        }

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
                CurrentMap = loc.CurrentMap?.Name ?? string.Empty,
                LastMapVisited = loc.LastMapVisited?.Name ?? string.Empty,
                PlayerLocX = loc.playerLoc.x,
                PlayerLocY = loc.playerLoc.y,
                IsSurfing = loc.IsSurfing ? 1 : 0,

                HasRunningShoes =
                    player.trainerItemDomain.HasRunningShoes ? 1 : 0,
            };
        }

        private static List<BadgeData> ExtractBadges(PlayerDomain player)
        {
            return player.Badges
                .Select(b => new BadgeData
                {
                    Id = b.Id,
                    IsObtained = b.IsObtained ? 1 : 0,
                })
                .ToList();
        }

        private static List<BagInventoryData> ExtractBagInventory(PlayerDomain player)
        {
            return player.trainerItemDomain.BagInventory
                .Where(kv => kv.Key != null && kv.Key.Id > 0 && kv.Value > 0)
                .Select(kv => new BagInventoryData
                {
                    ItemId = kv.Key.Id,
                    Quantity = kv.Value,
                })
                .ToList();
        }

        private static List<PokedexData> ExtractPokedex(PlayerDomain player)
        {
            return player.Pokedex
                .Select(kv => new PokedexData
                {
                    PokedexId = kv.Key,
                    Seen = kv.Value.seen ? 1 : 0,
                    Caught = kv.Value.caught ? 1 : 0,
                })
                .ToList();
        }
    }
}