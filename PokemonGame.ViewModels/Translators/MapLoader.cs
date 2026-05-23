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
using PokemonGame.Services.Factory;
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

        private MapDomain BuildDomain(MapBundle bundle)
        {
            if (_cycleCache.TryGetValue(bundle.Map.Id, out var existing))
                return existing;

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

            // ── Connected maps ─────────────────────────────────────────────
            foreach (var conn in bundle.Connections)
            {
                var connectedBundle = _mapService.GetMap(conn.ConnectedMapId);

                if (connectedBundle == null)
                    continue;

                if (!Enum.IsDefined(typeof(ConnectionDirection), conn.Direction))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MapLoader] Skipping connection id={conn.Id}: unknown Direction={conn.Direction}");

                    continue;
                }

                domain.ConnectedMaps.Add(new ConnectedMapDomain
                {
                    ConnectedMap = BuildDomain(connectedBundle),
                    ConnectionDirection = (ConnectionDirection)conn.Direction,
                    Margin = conn.Margin,
                });
            }

            // ── Encounters ─────────────────────────────────────────────────
            // BUG-098 fix:
            // Create the translator once, not inside every encounter loop.
            var moveTranslator = new MoveTranslator(ServiceFactory.Instance.MoveService);
            var abilityTranslator = new AbilityTranslator(ServiceFactory.Instance.AbilityService);
            var itemTranslator = new ItemTranslator(
                ServiceFactory.Instance.ItemService,
                moveTranslator);

            var teamTranslator = new TeamTranslator(
                _pokemonService,
                moveTranslator,
                abilityTranslator,
                itemTranslator);

            foreach (var encounter in bundle.Encounters)
            {
                var pokemon = _pokemonService.GenerateWildPokemon(encounter);

                if (pokemon == null)
                    continue;

                PokemonState pokemonState = teamTranslator.TranslateToDomain(pokemon);

                domain.Encounters.Add(new EncounterDomain
                {
                    Pokemon = pokemonState,

                    MinLevel = encounter.MinLevel,
                    MaxLevel = encounter.MaxLevel,

                    CatchChance = encounter.CatchChance,
                    Rate = encounter.Rate,

                    evYield =
                        encounter.EvYieldAmount > 0
                            ? ((Stat)encounter.EvYieldStat, encounter.EvYieldAmount)
                            : null,

                    BaseExpYield = encounter.BaseExpYield,
                    BaseFriendshipYield = encounter.BaseFriendshipYield,

                    CatchRate = encounter.CatchRate,
                    femaleRatio = encounter.FemaleRatio,

                    // TODO:
                    // BUG-097 cannot be fully fixed until GrowthRate exists
                    // in encounter/species data. Keep the fallback explicit.
                    GrowthRate = GrowthRateType.MediumFast,
                });
            }

            // ── Warps / wraps ──────────────────────────────────────────────
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

            // ── NPCs ───────────────────────────────────────────────────────
            foreach (var spawn in bundle.NpcSpawns)
            {
                domain.Npc.Add(BuildNpc(spawn));
            }

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
            // BUG-095:
            // PlayerID and UserID are account/save identifiers.
            // TrainerID is the in-game trainer number.
            PlayerID = UserStore.Instance.PlayerID,
            UserID = UserStore.Instance.UserID,
        };

        private static TrainerInfoData BuildTrainerInfo(PlayerDomain player) => new()
        {
            PlayerID = player.trainerInfo.TrainerID,
            TrainerID = player.trainerInfo.TrainerID,
            Name = player.trainerInfo.Name,
            Money = player.trainerInfo.Money,
            TimePlayed = player.trainerInfo.TimePlayed.TimeOfDay.ToString(@"hh\:mm\:ss"),
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
    }
}