using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    public class BattleTerrainService
    {
        private readonly BattleState _battle;
        private readonly BattleLogger _logger;

        public TerrainType CurrentTerrain { get; private set; } = TerrainType.None;
        public int TurnsRemaining { get; private set; } = 0;

        public BattleTerrainService(BattleState battle, BattleLogger logger)
        {
            _battle = battle;
            _logger = logger;
        }

        public void SetTerrain(TerrainType terrain, int duration = 5)
        {
            if (CurrentTerrain == terrain)
            {
                return;
            }

            CurrentTerrain = terrain;
            TurnsRemaining = duration;
            _logger.Log($"The battlefield became {terrain} Terrain!");
        }

        public void TickTerrain()
        {
            if (CurrentTerrain == TerrainType.None)
            {
                return;
            }

            TurnsRemaining--;
            if (TurnsRemaining <= 0)
            {
                _logger.Log($"The {CurrentTerrain} Terrain faded.");
                CurrentTerrain = TerrainType.None;
            }
        }

        public void ClearTerrain()
        {
            CurrentTerrain = TerrainType.None;
            TurnsRemaining = 0;
        }
    }
}