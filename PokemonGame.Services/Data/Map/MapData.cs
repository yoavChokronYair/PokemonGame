using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.Map
{
    public class MapData
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Width { get; set; }
        public int Height { get; set; }

        public string Song { get; set; } = string.Empty;

        public int FlyWrapX { get; set; }
        public int FlyWrapY { get; set; }

        public int TownMapX { get; set; }
        public int TownMapY { get; set; }
    }
    public class TilesetData
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public int TileWidth { get; set; }
        public int TileHeight { get; set; }

        public int Margin { get; set; }
        public int Spacing { get; set; }
    }
    public class TileMetadataData
    {
        public int TilesetId { get; set; }

        public int TileId { get; set; }

        public int CollisionType { get; set; }

        public int TileType { get; set; }

        public bool IsAnimated { get; set; }

        public int AnimationFrames { get; set; }

        public int TilesetX { get; set; }

        public int TilesetY { get; set; }
    }
    public class MapTileData
    {
        public int Id { get; set; }

        public int MapId { get; set; }

        public int LayerType { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int TilesetId { get; set; }

        public int TileId { get; set; }
    }
    public class MapCollisionObjectData
    {
        public int Id { get; set; }

        public int MapId { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int CollisionType { get; set; }
    }
    public class ConnectedMapData
    {
        public int Id { get; set; }

        public int MapId { get; set; }

        public int ConnectedMapId { get; set; }

        public int Direction { get; set; }

        public int Margin { get; set; }
    }
    public class WrapData
    {
        public int Id { get; set; }

        public int MapId { get; set; }

        public int TargetMapId { get; set; }

        public int WrapX { get; set; }

        public int WrapY { get; set; }

        public int SpawnRow { get; set; }

        public int SpawnCol { get; set; }
    }
    public class EncounterData
    {
        public int Id { get; set; }

        public int MapId { get; set; }

        public int PokemonId { get; set; }

        public int MinLevel { get; set; }

        public int MaxLevel { get; set; }

        public int CatchChance { get; set; }

        public int Rate { get; set; }

        public int EvYieldStat { get; set; }

        public int EvYieldAmount { get; set; }

        public int BaseExpYield { get; set; }

        public int BaseFriendshipYield { get; set; }

        public int CatchRate { get; set; }

        public int FemaleRatio { get; set; }
    }
    public class NpcSpawnData
    {
        public int Id { get; set; }

        public int MapId { get; set; }

        public int NpcId { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int CollisionType { get; set; }

        public int MovementType { get; set; }

        public int FacingDirection { get; set; }

        public int DirectionA { get; set; }

        public int DirectionB { get; set; }

        public int StepsPerLeg { get; set; }

        public bool DefaultState { get; set; }

        public bool IsDisappearing { get; set; }

        public int VisionRange { get; set; }

        public int VisionType { get; set; }
    }
}
