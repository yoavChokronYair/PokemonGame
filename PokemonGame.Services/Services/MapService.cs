using System;
using System.Collections.Generic;
using System.Linq;
using PokemonGame.Services.Data.Map;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Services
{
    // ─────────────────────────────────────────────────────────────
    // RESULT TYPES
    // ─────────────────────────────────────────────────────────────

    public sealed class MapBundle
    {
        public MapData Map { get; set; } = null!;
        public TilesetData? Tileset { get; set; }
        public IReadOnlyList<TileMetadataData> TileMeta { get; set; } = new List<TileMetadataData>();
        public IReadOnlyList<MapTileData> Tiles { get; set; } = new List<MapTileData>();
        public IReadOnlyList<MapCollisionObjectData> Collisions { get; set; } = new List<MapCollisionObjectData>();
        public IReadOnlyList<ConnectedMapData> Connections { get; set; } = new List<ConnectedMapData>();
        public IReadOnlyList<WrapData> Wraps { get; set; } = new List<WrapData>();
        public IReadOnlyList<EncounterData> Encounters { get; set; } = new List<EncounterData>();
        public IReadOnlyList<NpcSpawnData> NpcSpawns { get; set; } = new List<NpcSpawnData>();

        // ── NPC SYSTEM ─────────────────────────────
        public IReadOnlyList<NpcDefinitionsData> NpcDefinitions { get; set; } = new List<NpcDefinitionsData>();
        public IReadOnlyList<DialogueSetsData> DialogueSets { get; set; } = new List<DialogueSetsData>();
        public IReadOnlyList<DialogueNodesData> DialogueNodes { get; set; } = new List<DialogueNodesData>();
        public IReadOnlyList<DialogueEdgesData> DialogueEdges { get; set; } = new List<DialogueEdgesData>();
    }

    public sealed class MapLayers
    {
        public IReadOnlyList<MapTileData> Background { get; set; } = new List<MapTileData>();
        public IReadOnlyList<MapTileData> Foreground { get; set; } = new List<MapTileData>();
    }

    // ─────────────────────────────────────────────────────────────
    // INTERFACE
    // ─────────────────────────────────────────────────────────────

    public interface IMapService
    {
        MapBundle? GetMap(int mapId);
        MapBundle? GetMap(string mapName);
        MapLayers GetLayers(int mapId);
        IReadOnlyList<MapCollisionObjectData> GetCollisions(int mapId);
        IReadOnlyList<ConnectedMapData> GetConnections(int mapId);
        IReadOnlyList<WrapData> GetWraps(int mapId);
        IReadOnlyList<EncounterData> GetEncounters(int mapId);
        IReadOnlyList<NpcSpawnData> GetNpcSpawns(int mapId);

        IReadOnlyDictionary<int, TileMetadataData> GetTileMetaLookup(int tilesetId);
        IReadOnlyList<MapData> GetAllMaps();
        bool MapExists(int mapId);
    }

    // ─────────────────────────────────────────────────────────────
    // IMPLEMENTATION
    // ─────────────────────────────────────────────────────────────

    public sealed class MapService : IMapService
    {
        private const int LayerBackground = 0;
        private const int LayerForeground = 1;

        private readonly MapRepository _maps;
        private readonly TilesetRepository _tilesets;
        private readonly TileMetadataRepository _tileMeta;
        private readonly MapTileRepository _mapTiles;
        private readonly MapCollisionRepository _collisions;
        private readonly ConnectedMapRepository _connections;
        private readonly WrapRepository _wraps;
        private readonly EncounterRepository _encounters;
        private readonly NpcSpawnRepository _npcSpawns;

        // ── NPC SYSTEM REPOS ─────────────────────────────
        private readonly NpcDefinitionsRepository _npcDefinitions;
        private readonly DialogueSetsRepository _dialogueSets;
        private readonly DialogueNodesRepository _dialogueNodes;
        private readonly DialogueEdgesRepository _dialogueEdges;

        public MapService()
        {
            var factory = ServiceFactory.Instance;

            _maps = factory.MapRepository;
            _tilesets = factory.TilesetRepository;
            _tileMeta = factory.TileMetadataRepository;
            _mapTiles = factory.MapTileRepository;
            _collisions = factory.MapCollisionRepository;
            _connections = factory.ConnectedMapRepository;
            _wraps = factory.WrapRepository;
            _encounters = factory.EncounterRepository;
            _npcSpawns = factory.NpcSpawnRepository;

            // NPC SYSTEM
            _npcDefinitions = factory.NpcDefinitionsRepository;
            _dialogueSets = factory.DialogueSetsRepository;
            _dialogueNodes = factory.DialogueNodesRepository;
            _dialogueEdges = factory.DialogueEdgesRepository;
        }

        // ─────────────────────────────────────────────────────────────
        // PUBLIC API
        // ─────────────────────────────────────────────────────────────

        public MapBundle? GetMap(int mapId)
        {
            var map = _maps.GetMapById(mapId);
            return map is null ? null : BuildBundle(map);
        }

        public MapBundle? GetMap(string mapName)
        {
            var map = _maps.GetMapByName(mapName);
            return map is null ? null : BuildBundle(map);
        }

        public MapLayers GetLayers(int mapId) => new()
        {
            Background = _mapTiles.GetTilesForLayer(mapId, LayerBackground),
            Foreground = _mapTiles.GetTilesForLayer(mapId, LayerForeground),
        };

        public IReadOnlyList<MapCollisionObjectData> GetCollisions(int mapId) =>
            _collisions.GetCollisionsForMap(mapId);

        public IReadOnlyList<ConnectedMapData> GetConnections(int mapId) =>
            _connections.GetConnectionsForMap(mapId);

        public IReadOnlyList<WrapData> GetWraps(int mapId) =>
            _wraps.GetWrapsForMap(mapId);

        public IReadOnlyList<EncounterData> GetEncounters(int mapId) =>
            _encounters.GetEncountersForMap(mapId);

        public IReadOnlyList<NpcSpawnData> GetNpcSpawns(int mapId) =>
            _npcSpawns.GetNpcSpawnsForMap(mapId);

        public IReadOnlyDictionary<int, TileMetadataData> GetTileMetaLookup(int tilesetId)
        {
            var list = _tileMeta.GetMetadataForTileset(tilesetId);
            return list.ToDictionary(m => m.TileId);
        }

        public IReadOnlyList<MapData> GetAllMaps() =>
            _maps.GetAllMaps();

        public bool MapExists(int mapId) =>
            _maps.MapExists(mapId);

        // ─────────────────────────────────────────────────────────────
        // BUILD FULL MAP
        // ─────────────────────────────────────────────────────────────

        private MapBundle BuildBundle(MapData map)
        {
            var tiles = _mapTiles.GetTilesForMap(map.Id);
            var tileset = ResolveTileset(tiles);

            var meta = tileset is null
                ? new List<TileMetadataData>()
                : _tileMeta.GetMetadataForTileset(tileset.Id);

            var npcSpawns = _npcSpawns.GetNpcSpawnsForMap(map.Id);

            var npcIds = npcSpawns.Select(n => n.NpcId).Distinct().ToList();

            var npcs = npcIds
                .Select(id => _npcDefinitions.Load(id))
                .Where(n => n != null)
                .ToList()!;

            var sets = npcs
                .SelectMany(n => _dialogueSets.LoadByNpc(n.Id))
                .ToList();

            var nodes = sets
                .SelectMany(s => _dialogueNodes.LoadBySet(s.Id))
                .ToList();

            var edges = nodes
                .SelectMany(n => _dialogueEdges.LoadByFromNode(n.Id))
                .ToList();

            return new MapBundle
            {
                Map = map,
                Tileset = tileset,
                TileMeta = meta,
                Tiles = tiles,
                Collisions = _collisions.GetCollisionsForMap(map.Id),
                Connections = _connections.GetConnectionsForMap(map.Id),
                Wraps = _wraps.GetWrapsForMap(map.Id),
                Encounters = _encounters.GetEncountersForMap(map.Id),
                NpcSpawns = npcSpawns,

                // NPC SYSTEM
                NpcDefinitions = npcs,
                DialogueSets = sets,
                DialogueNodes = nodes,
                DialogueEdges = edges
            };
        }

        private TilesetData? ResolveTileset(IReadOnlyList<MapTileData> tiles)
        {
            if (tiles.Count == 0) return null;

            var representative =
                tiles.FirstOrDefault(t => t.LayerType == LayerBackground)
                ?? tiles[0];

            return _tilesets.GetTilesetById(representative.TilesetId);
        }
    }
}