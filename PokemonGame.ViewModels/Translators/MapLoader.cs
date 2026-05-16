using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Npc;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Data.Map;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;
using PokemonGame.Services.Services;
using PokemonGame.ViewModels.Store;

namespace PokemonGame.ViewModels.Translators
{
    public sealed class MapLoader
    {
        private readonly IMapService _mapService;
        private readonly IPokemonService _pokemonService;
        private static readonly Dictionary<string, MapDomain> _sessionCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, MapDomain> _cycleCache = new();

        public MapLoader(IMapService mapService, IPokemonService pokemonService)
        {
            _mapService = mapService;
            _pokemonService = pokemonService;
        }

        // ── Map Loading ───────────────────────────────────────────────────────

        public MapDomain Load(string mapName)
        {
            if (_sessionCache.TryGetValue(mapName, out var cached)) return cached;
            _cycleCache.Clear();
            var bundle = _mapService.GetMap(mapName)
                ?? throw new InvalidOperationException($"Map '{mapName}' not found.");
            var domain = BuildDomain(bundle);
            _sessionCache[mapName] = domain;
            return domain;
        }

        public static void InvalidateCache(string mapName) => _sessionCache.Remove(mapName);
        public static void InvalidateAll() => _sessionCache.Clear();

        private MapDomain BuildDomain(MapBundle bundle)
        {
            if (_cycleCache.TryGetValue(bundle.Map.Id, out var existing)) return existing;

            var domain = new MapDomain
            {
                Name = bundle.Map.Name,
                Width = bundle.Map.Width,
                Height = bundle.Map.Height,
                FlyWrapLoc = (bundle.Map.FlyWrapX, bundle.Map.FlyWrapY),
                TownMapLoc = (bundle.Map.TownMapX, bundle.Map.TownMapY),
                BackgroundBlocks = BuildTiles(bundle.Tiles, TileLayerType.Ground),
                Blocks = BuildTiles(bundle.Tiles, TileLayerType.Objects),
                CollisionObjects = BuildCollisionObjects(bundle.Collisions),
                ConnectedMaps = new List<ConnectedMapDomain>(),
                Encounters = new List<EncounterDomain>(),
                Wraps = new List<WrapDomain>(),
                Npc = new List<NpcObjectDomain>(),
            };

            _cycleCache[bundle.Map.Id] = domain;

            foreach (var conn in bundle.Connections)
            {
                var nb = _mapService.GetMap(conn.ConnectedMapId);
                if (nb == null) continue;
                if (!Enum.IsDefined(typeof(ConnectionDirection), conn.Direction))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MapLoader] Skipping connection id={conn.Id}: unknown Direction={conn.Direction}");
                    continue;
                }
                domain.ConnectedMaps.Add(new ConnectedMapDomain
                {
                    ConnectedMap = BuildDomain(nb),
                    ConnectionDirection = (ConnectionDirection)conn.Direction,
                    Margin = conn.Margin,
                });
            }
            foreach (var encounter in bundle.Encounters)
            {
                var pokemon = _pokemonService.GenerateWildPokemon(encounter);
                TeamTranslator translator = new();
                if (pokemon == null)
                    continue;

                domain.Encounters.Add(new EncounterDomain
                {
                    Pokemon = translator.TranslateToDomain(pokemon),

                    MinLevel = encounter.MinLevel,
                    MaxLevel = encounter.MaxLevel,

                    CatchChance = encounter.CatchChance,
                    Rate = encounter.Rate,

                    evYield =
                        encounter.EvYieldAmount > 0
                            ? ((Stat)encounter.EvYieldStat,
                               encounter.EvYieldAmount)
                            : null,

                    BaseExpYield = encounter.BaseExpYield,
                    BaseFriendshipYield = encounter.BaseFriendshipYield,

                    CatchRate = encounter.CatchRate,

                    femaleRatio = encounter.FemaleRatio,

                    GrowthRate = GrowthRateType.MediumFast,
                });
            }
            foreach (var wrap in bundle.Wraps)
            {
                var tb = _mapService.GetMap(wrap.TargetMapId);
                if (tb == null) continue;
                domain.Wraps.Add(new WrapDomain
                {
                    WrapLoc = (wrap.WrapX / 2, wrap.WrapY / 2),
                    TargetMap = BuildDomain(tb),
                    SpawnLoc = (wrap.SpawnRow, wrap.SpawnCol),
                });
            }

            foreach (var spawn in bundle.NpcSpawns)
                domain.Npc.Add(BuildNpc(spawn));

            return domain;
        }

        private enum TileLayerType { Ground = 0, Water = 1, Objects = 2, Above = 3 }

        private static List<TileDomain> BuildTiles(IReadOnlyList<MapTileData> tiles, TileLayerType layer)
        {
            var result = new List<TileDomain>();
            foreach (var t in tiles)
            {
                if (t.LayerType != (int)layer) continue;
                result.Add(new TileDomain { Tileid = t.TileId, X = t.X, Y = t.Y });
            }
            return result;
        }

        private static List<CollisionObjectDomain> BuildCollisionObjects(IReadOnlyList<MapCollisionObjectData> rows)
        {
            var result = new List<CollisionObjectDomain>(rows.Count);
            foreach (var r in rows)
            {
                if (!Enum.IsDefined(typeof(CollisionType), r.CollisionType))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MapLoader] Skipping collision id={r.Id}: unknown CollisionType={r.CollisionType}");
                    continue;
                }
                result.Add(new CollisionObjectDomain
                {
                    X = r.X,
                    Y = r.Y,
                    Width = r.Width,
                    Height = r.Height,
                    CollisionType = (CollisionType)r.CollisionType,
                });
            }
            return result;
        }

        private static NpcObjectDomain BuildNpc(NpcSpawnData spawn)
        {
            static T SafeCast<T>(int value, T fallback, string field, int spawnId)
                where T : struct, Enum
            {
                if (Enum.IsDefined(typeof(T), value)) return (T)(object)value;
                System.Diagnostics.Debug.WriteLine(
                    $"[MapLoader] NpcSpawn id={spawnId}: unknown {field}={value}, using {fallback}");
                return fallback;
            }

            return new NpcObjectDomain
            {
                NpcInfo = new NpcDomain { Id = spawn.NpcId },
                Location = (
                    spawn.X * MapConstants.TilesPerSquare,
                    spawn.Y * MapConstants.TilesPerSquare
                ),
                CollisionType = SafeCast(spawn.CollisionType, CollisionType.Blocked, nameof(spawn.CollisionType), spawn.Id),
                MovementType = SafeCast(spawn.MovementType, MovementType.Stationary, nameof(spawn.MovementType), spawn.Id),
                Direction = SafeCast(spawn.FacingDirection, FacingDirection.Down, nameof(spawn.FacingDirection), spawn.Id),
                DirectionA = SafeCast(spawn.DirectionA, FacingDirection.Down, nameof(spawn.DirectionA), spawn.Id),
                DirectionB = SafeCast(spawn.DirectionB, FacingDirection.Up, nameof(spawn.DirectionB), spawn.Id),
                StepsPerLeg = spawn.StepsPerLeg,
                VisionRange = spawn.VisionRange,
                VisionType = SafeCast(spawn.VisionType, VisionType.Normal, nameof(spawn.VisionType), spawn.Id),
            };
        }

        // ── Save ─────────────────────────────────────────────────────────────

        public void Save(IStoryPlayerService storyPlayerService)
        {
            var player = PlayerDomain.Instance;
            storyPlayerService.SaveAll(BuildSaveTree(player), UserStore.Instance.PlayerID);
        }

        private static StorySaveTree BuildSaveTree(PlayerDomain player) => new()
        {
            CurrentPlayer = BuildStoryPlayer(player),
            TrainerInfo = BuildTrainerInfo(player),
            Badges = BuildBadges(player),
            StoryFlags = player.ProgressFlags.StoryFlags.ToList(),
            DefeatedTrainers = player.ProgressFlags.DefeatedTrainers.ToList(),
            ItemsTaken = player.ProgressFlags.ItemTaken.ToList(),
            TradedPokemon = player.ProgressFlags.TradedPokemon.ToList(),
            BagInventory = BuildBagInventory(player),
            Pokedex = BuildPokedex(player),
            Party = BuildParty(player),
        };

        private static StoryPlayerData BuildStoryPlayer(PlayerDomain player) => new()
        {
            PlayerID = player.trainerInfo.TrainerID,
            UserID = player.trainerInfo.TrainerID,
        };

        private static TrainerInfoData BuildTrainerInfo(PlayerDomain player) => new()
        {
            PlayerID = player.trainerInfo.TrainerID,
            TrainerID = player.trainerInfo.TrainerID,
            Name = player.trainerInfo.Name,
            Money = player.trainerInfo.Money,
            TimePlayed = player.trainerInfo.TimePlayed.ToString(@"hh\:mm\:ss"),
            Gender = (int)player.trainerInfo.Gender,
            HallOfFameDebut = player.trainerInfo.HallOfFameDebut,
            FacingDirection = (int)player.trainerMapLocDomain.FacingDirection,
            CurrentMap = player.trainerMapLocDomain.CurrentMap?.Name ?? "",
            LastMapVisited = player.trainerMapLocDomain.LastMapVisited?.Name ?? "",
            PlayerLocX = player.trainerMapLocDomain.playerLoc.x,
            PlayerLocY = player.trainerMapLocDomain.playerLoc.y,
            IsSurfing = player.trainerMapLocDomain.IsSurfing ? 1 : 0,
            HasRunningShoes = player.trainerItemDomain.HasRunningShoes ? 1 : 0,
        };

        private static List<BadgeData> BuildBadges(PlayerDomain player) =>
            player.Badges.Select(b => new BadgeData
            {
                PlayerID = player.trainerInfo.TrainerID,
                Id = b.Id,
                IsObtained = b.IsObtained ? 1 : 0,
            }).ToList();

        private static List<BagInventoryData> BuildBagInventory(PlayerDomain player) =>
            player.trainerItemDomain.BagInventory
                .Select(kv => new BagInventoryData
                {
                    PlayerID = player.trainerInfo.TrainerID,
                    ItemId = kv.Key.Id,
                    Quantity = kv.Value,
                })
                .ToList();

        private static List<PokedexData> BuildPokedex(PlayerDomain player) =>
            player.Pokedex
                .Select(kv => new PokedexData
                {
                    PlayerID = player.trainerInfo.TrainerID,
                    PokedexId = kv.Key,
                    Seen = kv.Value.seen ? 1 : 0,
                    Caught = kv.Value.caught ? 1 : 0,
                })
                .ToList();

        private static List<StoryPlayerPokemonData> BuildParty(PlayerDomain player)
        {
            if (player.Team == null) return new();
            return player.Team.ActiveMembers
                .Select(p => new StoryPlayerPokemonData
                {
                    PlayerID = player.trainerInfo.TrainerID,
                    BattlerPokemonId = p.PokemonState.PokedexId,
                    Nickname = p.Nickname,
                    PokemonUID = p.PokemonUID,
                    OriginalTrainerID = p.OriginalTrainerID,
                    OriginalTrainerName = p.OriginalTrainerName,
                    ObtainMethod = (int)p.ObtainMethod,
                    ObtainedAtRoute = p.ObtainedAtRoute ?? "",
                    ObtainedAt = p.ObtainedAt,
                    ObtainedAtLevel = p.ObtainedAtLevel,
                    CaughtWithBall = (int)p.CaughtWithBall,
                    MetLocationText = p.MetLocationText ?? "",
                    Experience = p.Experience,
                    GrowthRate = p.GrowthRate.ToString(),
                    CurrentHP = p.CurrentHP,
                    StatusId = (int)p.PersistentStatus,
                    Friendship = p.Friendship,
                    Affection = p.Affection,
                })
                .ToList();
        }

        // ── Load ─────────────────────────────────────────────────────────────

        public void Load(IStoryPlayerService storyPlayerService, int userId)
        {
            var save = storyPlayerService.LoadAll(userId);
            ApplySaveTree(save);
        }

        private void ApplySaveTree(StorySaveTree save)
        {
            var player = PlayerDomain.Instance;
            ApplyTrainerInfo(player, save.TrainerInfo);
            ApplyBadges(player, save.Badges);
            ApplyProgressFlags(player, save);
            ApplyBagInventory(player, save.BagInventory);
            ApplyPokedex(player, save.Pokedex);
            ApplyParty(player, save.Party);
        }

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
                player.trainerMapLocDomain.CurrentMap = Load(data.CurrentMap);

            if (!string.IsNullOrEmpty(data.LastMapVisited))
                player.trainerMapLocDomain.LastMapVisited = Load(data.LastMapVisited);

            player.trainerItemDomain.HasRunningShoes = data.HasRunningShoes == 1;
        }

        private static void ApplyBadges(PlayerDomain player, List<BadgeData> badges)
        {
            player.Badges.Clear();
            foreach (var b in badges)
                player.Badges.Add(new BadgeDomain { Id = b.Id, IsObtained = b.IsObtained == 1 });
        }

        private static void ApplyProgressFlags(PlayerDomain player, StorySaveTree save)
        {
            player.ProgressFlags.StoryFlags.Clear();
            foreach (var f in save.StoryFlags)
                player.ProgressFlags.StoryFlags.Add(f);

            player.ProgressFlags.DefeatedTrainers.Clear();
            foreach (var t in save.DefeatedTrainers)
                player.ProgressFlags.DefeatedTrainers.Add(t);

            player.ProgressFlags.ItemTaken.Clear();
            foreach (var i in save.ItemsTaken)
                player.ProgressFlags.ItemTaken.Add(i);

            player.ProgressFlags.TradedPokemon.Clear();
            foreach (var p in save.TradedPokemon)
                player.ProgressFlags.TradedPokemon.Add(p);
        }

        private static void ApplyBagInventory(PlayerDomain player, List<BagInventoryData> inventory)
        {
            player.trainerItemDomain.BagInventory.Clear();
            foreach (var entry in inventory)
            {
                var item = new itemsDomain { Id = entry.ItemId };
                player.trainerItemDomain.BagInventory[item] = entry.Quantity;
            }
        }

        private static void ApplyPokedex(PlayerDomain player, List<PokedexData> pokedex)
        {
            player.Pokedex.Clear();
            foreach (var entry in pokedex)
            {
                player.Pokedex[entry.PokedexId] = (
                    seen: entry.Seen == 1,
                    caught: entry.Caught == 1,
                    name: entry.PokedexId.ToString() // replace with species name lookup
                );
            }
        }

        private static void ApplyParty(PlayerDomain player, List<StoryPlayerPokemonData> party)
        {
            player.Team = new PlayerTeamDomain();
            foreach (var data in party)
            {
                var pokemon = new PokemonPlayerDomain
                {
                    PokemonUID = data.PokemonUID,
                    Nickname = data.Nickname,
                    OriginalTrainerID = data.OriginalTrainerID,
                    OriginalTrainerName = data.OriginalTrainerName,
                    ObtainMethod = (ObtainMethodType)data.ObtainMethod,
                    ObtainedAtRoute = data.ObtainedAtRoute,
                    ObtainedAt = data.ObtainedAt,
                    ObtainedAtLevel = data.ObtainedAtLevel,
                    CaughtWithBall = (PokeBallType)data.CaughtWithBall,
                    MetLocationText = data.MetLocationText,
                    Experience = data.Experience,
                    GrowthRate = Enum.TryParse<GrowthRateType>(data.GrowthRate, out var gr)
                                            ? gr : GrowthRateType.MediumFast,
                    CurrentHP = data.CurrentHP,
                    PersistentStatus = (StatusCondition)data.StatusId,
                    Friendship = data.Friendship,
                    Affection = data.Affection,
                };
                player.Team.TryAdd(pokemon);
            }
        }
    }
}