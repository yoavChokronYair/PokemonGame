using System;
using System.Collections.Generic;
using System.Text;
using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.Map;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Services
{
    // ── Result types the VM works with ──────────────────────────────────────

    /// <summary>
    /// Everything the VM needs to render and interact with one map.
    /// </summary>
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
    }

    /// <summary>
    /// Tile data split by layer, ready for the viewport builder.
    /// </summary>
    public sealed class MapLayers
    {
        public IReadOnlyList<MapTileData> Background { get; set; } = new List<MapTileData>();
        public IReadOnlyList<MapTileData> Foreground { get; set; } = new List<MapTileData>();
    }

    // ── Interface ────────────────────────────────────────────────────────────

    public interface IMapService
    {
        /// <summary>Loads the full bundle for a map by id.</summary>
        MapBundle? GetMap(int mapId);

        /// <summary>Loads the full bundle for a map by name.</summary>
        MapBundle? GetMap(string mapName);

        /// <summary>Returns background and foreground tile lists for a map.</summary>
        MapLayers GetLayers(int mapId);

        /// <summary>Returns collision objects for a map.</summary>
        IReadOnlyList<MapCollisionObjectData> GetCollisions(int mapId);

        /// <summary>Returns all connected-map descriptors for a map.</summary>
        IReadOnlyList<ConnectedMapData> GetConnections(int mapId);

        /// <summary>Returns all warps defined on a map.</summary>
        IReadOnlyList<WrapData> GetWraps(int mapId);

        /// <summary>Returns wild encounter table for a map.</summary>
        IReadOnlyList<EncounterData> GetEncounters(int mapId);

        /// <summary>Returns NPC spawn records for a map.</summary>
        IReadOnlyList<NpcSpawnData> GetNpcSpawns(int mapId);

        /// <summary>
        /// Returns the tile-metadata lookup for every tile in a tileset,
        /// keyed by TileId. Useful for collision/type lookups in the VM.
        /// </summary>
        IReadOnlyDictionary<int, TileMetadataData> GetTileMetaLookup(int tilesetId);

        /// <summary>Returns a flat list of all maps (for map-select screens, etc.).</summary>
        IReadOnlyList<MapData> GetAllMaps();

        /// <summary>Checks whether a map with the given id exists.</summary>
        bool MapExists(int mapId);
    }

    // ── Implementation ───────────────────────────────────────────────────────

    public sealed class MapService : IMapService
    {
        // Layer values stored in MapTileData.LayerType
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

        public MapService()
        {
            ServiceFactory factory = ServiceFactory.Instance;
            _maps = factory.MapRepository;
            _tilesets = factory.TilesetRepository;
            _tileMeta = factory.TileMetadataRepository;
            _mapTiles = factory.MapTileRepository;
            _collisions = factory.MapCollisionRepository;
            _connections = factory.ConnectedMapRepository;
            _wraps = factory.WrapRepository;
            _encounters = factory.EncounterRepository;
            _npcSpawns = factory.NpcSpawnRepository;
        }

        // ── IMapService ──────────────────────────────────────────────────────

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

        // ── Private helpers ──────────────────────────────────────────────────

        /// Assembles a full <see cref="MapBundle"/> from a known <see cref="MapData"/>.
        private MapBundle BuildBundle(MapData map)
        {
            // Tiles carry TilesetId — grab the first one we find (maps typically
            // use a single tileset; extend here if multi-tileset support is needed).
            var tiles = _mapTiles.GetTilesForMap(map.Id);
            var tileset = ResolveTileset(tiles);
            var meta = tileset is null
                ? new List<TileMetadataData>()
                : _tileMeta.GetMetadataForTileset(tileset.Id);

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
                NpcSpawns = _npcSpawns.GetNpcSpawnsForMap(map.Id),
            };
        }

        /// Picks the tileset used by the map's tile data.
        private TilesetData? ResolveTileset(IReadOnlyList<MapTileData> tiles)
        {
            if (tiles.Count == 0) return null;

            // Use the tileset referenced by the first background tile, falling
            // back to any tile if no background tile exists.
            var representative =
                tiles.FirstOrDefault(t => t.LayerType == LayerBackground)
                ?? tiles[0];

            return _tilesets.GetTilesetById(representative.TilesetId);
        }
    }
}

