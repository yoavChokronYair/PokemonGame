using System;
using System.Collections.Generic;
using System.Text;
using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.Map;

namespace PokemonGame.Services.Data.Repositories
{
    internal class MapRepository : DbRepository<int, MapData>
    {
        internal MapRepository(IDbConnectionService db) : base(db) { }

        public MapData? GetMapById(int id) =>
            GetCached(id, () =>
                _db.QuerySingle<MapData>(
                    "SELECT * FROM Maps WHERE Id = @id",
                    new { id }));

        public MapData? GetMapByName(string name) =>
            _db.QuerySingle<MapData>(
                "SELECT * FROM Maps WHERE Name = @name",
                new { name });

        public List<MapData> GetAllMaps() =>
            GetAllCached(
                () => _db.Query<MapData>(
                    "SELECT * FROM Maps ORDER BY Id ASC"),
                m => m.Id);

        public bool MapExists(int id) =>
            _db.QueryScalar<long>(
                "SELECT COUNT(*) FROM Maps WHERE Id = @id",
                new { id }) > 0;
    }
    internal class TilesetRepository : DbRepository<int, TilesetData>
    {
        internal TilesetRepository(IDbConnectionService db) : base(db) { }

        public TilesetData? GetTilesetById(int id) =>
            GetCached(id, () =>
                _db.QuerySingle<TilesetData>(
                    "SELECT * FROM Tilesets WHERE Id = @id",
                    new { id }));

        public TilesetData? GetTilesetByName(string name) =>
            _db.QuerySingle<TilesetData>(
                "SELECT * FROM Tilesets WHERE Name = @name",
                new { name });

        public List<TilesetData> GetAllTilesets() =>
            GetAllCached(
                () => _db.Query<TilesetData>(
                    "SELECT * FROM Tilesets ORDER BY Id ASC"),
                t => t.Id);
    }
    internal class TileMetadataRepository : DbRepository<int, TileMetadataData>
    {
        internal TileMetadataRepository(IDbConnectionService db) : base(db) { }

        public TileMetadataData? GetTileMetadataById(int id) =>
            GetCached(id, () =>
                _db.QuerySingle<TileMetadataData>(
                    "SELECT * FROM TileMetadata WHERE Id = @id",
                    new { id }));

        public List<TileMetadataData> GetMetadataForTileset(int tilesetId) =>
            _db.Query<TileMetadataData>(
                @"SELECT * 
                  FROM TileMetadata 
                  WHERE TilesetId = @tilesetId 
                  ORDER BY TileId ASC",
        new { tilesetId });

        public TileMetadataData? GetTileMetadata(int tilesetId, int tileId) =>
            _db.QuerySingle<TileMetadataData>(
                @"
                SELECT *
                FROM TileMetadata
                WHERE TilesetId = @tilesetId
                AND TileId = @tileId
                ",
                new { tilesetId, tileId });
    }
    internal class MapTileRepository : DbRepository<int, MapTileData>
    {
        internal MapTileRepository(IDbConnectionService db) : base(db) { }

        public MapTileData? GetMapTileById(int id) =>
            GetCached(id, () =>
                _db.QuerySingle<MapTileData>(
                    "SELECT * FROM MapTiles WHERE Id = @id",
                    new { id }));

        public List<MapTileData> GetTilesForMap(int mapId) =>
            _db.Query<MapTileData>(
                 @"
                SELECT *
                FROM MapTiles
                WHERE MapId = @mapId
                ORDER BY Y ASC, X ASC
                ",
                new { mapId });

        public List<MapTileData> GetTilesForLayer(int mapId, int layerType) =>
            _db.Query<MapTileData>(
                @"
                SELECT *
                FROM MapTiles
                WHERE MapId = @mapId
                AND LayerType = @layerType
                ORDER BY Y ASC, X ASC
                ",
                new { mapId, layerType });
    }
    internal class MapCollisionRepository : DbRepository<int, MapCollisionObjectData>
    {
        internal MapCollisionRepository(IDbConnectionService db) : base(db) { }

        public MapCollisionObjectData? GetCollisionById(int id) =>
            GetCached(id, () =>
                _db.QuerySingle<MapCollisionObjectData>(
                    "SELECT * FROM MapCollisionObjects WHERE Id = @id",
                    new { id }));

        public List<MapCollisionObjectData> GetCollisionsForMap(int mapId) =>
            _db.Query<MapCollisionObjectData>(
                @"
                SELECT *
                FROM MapCollisionObjects
                WHERE MapId = @mapId
                ORDER BY Y ASC, X ASC
                ",
                new { mapId });
    }
    internal class ConnectedMapRepository : DbRepository<int, ConnectedMapData>
    {
        internal ConnectedMapRepository(IDbConnectionService db) : base(db) { }

        public ConnectedMapData? GetConnectedMapById(int id) =>
            GetCached(id, () =>
                _db.QuerySingle<ConnectedMapData>(
                    "SELECT * FROM ConnectedMaps WHERE Id = @id",
                    new { id }));

        public List<ConnectedMapData> GetConnectionsForMap(int mapId) =>
            _db.Query<ConnectedMapData>(
                @"
                SELECT *
                FROM ConnectedMaps
                WHERE MapId = @mapId
                ",
                new { mapId });
    }
    internal class WrapRepository : DbRepository<int, WrapData>
    {
        internal WrapRepository(IDbConnectionService db) : base(db) { }

        public WrapData? GetWrapById(int id) =>
            GetCached(id, () =>
                _db.QuerySingle<WrapData>(
                    "SELECT * FROM Wraps WHERE Id = @id",
                    new { id }));

        public List<WrapData> GetWrapsForMap(int mapId) =>
            _db.Query<WrapData>(
                 @"
                SELECT *
                FROM Wraps
                WHERE MapId = @mapId
                ",
                new { mapId });
    }
    internal class EncounterRepository : DbRepository<int, EncounterData>
    {
        internal EncounterRepository(IDbConnectionService db) : base(db) { }

        public EncounterData? GetEncounterById(int id) =>
            GetCached(id, () =>
                _db.QuerySingle<EncounterData>(
                    "SELECT * FROM Encounters WHERE Id = @id",
                    new { id }));

        public List<EncounterData> GetEncountersForMap(int mapId) =>
            _db.Query<EncounterData>(
                @"
                SELECT *
                FROM Encounters
                WHERE MapId = @mapId
                ",
                new { mapId });
    }
    internal class NpcSpawnRepository : DbRepository<int, NpcSpawnData>
    {
        internal NpcSpawnRepository(IDbConnectionService db) : base(db)
        {
        }

        public NpcSpawnData? GetNpcSpawnById(int id)
        {
            return _db.QuerySingle<NpcSpawnData>(
                @"
            SELECT
                Id,
                MapId,
                NpcId,
                X,
                Y,
                CollisionType,
                MovementType,
                FacingDirection,
                COALESCE(DirectionA, 0) AS DirectionA,
                COALESCE(DirectionB, 0) AS DirectionB,
                COALESCE(StepsPerLeg, 0) AS StepsPerLeg,
                DefaultState,
                IsDisappearing,
                VisionRange,
                VisionType
            FROM NpcSpawns
            WHERE Id = @id
            ",
                new { id });
        }

        public List<NpcSpawnData> GetNpcSpawnsForMap(int mapId)
        {
            var result = _db.Query<NpcSpawnData>(
                @"
            SELECT
                Id,
                MapId,
                NpcId,
                X,
                Y,
                CollisionType,
                MovementType,
                FacingDirection,
                COALESCE(DirectionA, 0) AS DirectionA,
                COALESCE(DirectionB, 0) AS DirectionB,
                COALESCE(StepsPerLeg, 0) AS StepsPerLeg,
                DefaultState,
                IsDisappearing,
                VisionRange,
                VisionType
            FROM NpcSpawns
            WHERE MapId = @mapId
            ORDER BY Id
            ",
                new { mapId });

            System.Diagnostics.Debug.WriteLine(
                $"[NpcSpawnRepository] mapId={mapId}, loaded={result.Count}");

            foreach (var npc in result)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[NpcSpawnRepository] SpawnId={npc.Id}, MapId={npc.MapId}, NpcId={npc.NpcId}, X={npc.X}, Y={npc.Y}");
            }

            return result;
        }
    }


}
