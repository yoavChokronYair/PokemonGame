using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.PokemonHelper;

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    public class BattleState
    {
        private readonly BattleDomain _state;
        public BattleWeatherService WeatherService { get; }
        public BattleStatusService StatusService { get; }
        public BattleTerrainService TerrainService { get; }
        public BattleLogger Logger { get; } = new();
        public BattleTurnResolver TurnResolver { get; }
        public BattleFieldState Field { get; } = new();
        public bool IsGravityActive => Field.IsGravityActive;

        public BattleState(BattleDomain state)
        {
            _state = state;
            WeatherService = new BattleWeatherService(this, Logger);
            StatusService = new BattleStatusService(Logger);
            TurnResolver = new BattleTurnResolver();
            TerrainService = new BattleTerrainService(this, Logger); // Initialize
        }

        // Data accessors
        public PokemonHelper.PokemonState Attacker => _state.Attacker;
        public PokemonHelper.PokemonState Defender => _state.Defender;
        public BattleSideState AttackerSide => _state.AttackerSide;
        public BattleSideState DefenderSide => _state.DefenderSide;
        public IMove? LastUsedMove => _state.LastUsedMove;
        public PokemonType? ActiveTypeOverride { get => _state.ActiveTypeOverride; set => _state.ActiveTypeOverride = value; }
        public int TurnNumber => _state.TurnNumber;
        public int LastDamageDealt { get => _state.LastDamageDealt; set => _state.LastDamageDealt = value; }

        // Logic
        public void RegisterMove(IMove move) => _state.LastUsedMove = move;

        public void BeginTurn()
        {
            _state.TurnNumber++;

            _state.LastDamageDealt = 0;
            Logger.LogTurnStart($"--- Turn {_state.TurnNumber} ---");
            Logger.LogTurnStart($"what will {_state.Attacker.Name} do?");
        }
        public void UpdateActivePair(PokemonHelper.PokemonState attacker, PokemonHelper.PokemonState defender)
        {
            _state.Attacker = attacker;
            _state.Defender = defender;
        }
        public void EndTurn() // Remove parameters
        {
            WeatherService.TickWeather();
            TerrainService.TickTerrain();
            Field.Tick();
            AttackerSide.Tick();
            DefenderSide.Tick();

            // Use the internal state references
            StatusService.ApplyEndOfTurnStatus(Attacker);
            StatusService.ApplyEndOfTurnStatus(Defender);

            _state.Attacker.turnsActive++;
            _state.Defender.turnsActive++;
        }


        public bool AttackerMovesFirst(int attackerPriority, int defenderPriority)
            => TurnResolver.AttackerMovesFirst(_state.Attacker, _state.Defender, attackerPriority, defenderPriority);

        public BattleSideState GetSide(BattleSide side) => side == BattleSide.Attacker ? AttackerSide : DefenderSide;

        public bool IsBattleOver => _state.Attacker.IsFainted || _state.Defender.IsFainted;
        public PokemonHelper.PokemonState? Winner => _state.Attacker.IsFainted ? _state.Defender : _state.Defender.IsFainted ? _state.Attacker : null;
    }
}
