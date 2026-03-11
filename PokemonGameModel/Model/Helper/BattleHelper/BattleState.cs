using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    internal class BattleState
    {
        private readonly BattleDomain _state;
        public BattleWeatherService WeatherService { get; }
        public BattleStatusService StatusService { get; }
        public BattleLogger Logger { get; } = new();
        public BattleTurnResolver TurnResolver { get; }

        public BattleState(BattleDomain state)
        {
            _state = state;
            WeatherService = new BattleWeatherService(this, Logger);
            StatusService = new BattleStatusService(Logger);
            TurnResolver = new BattleTurnResolver();
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
            Logger.Log($"--- Turn {_state.TurnNumber} ---");
        }

        public void EndTurn()
        {
            WeatherService.TickWeather();
            AttackerSide.Tick();
            DefenderSide.Tick();
            StatusService.ApplyEndOfTurnStatus(Attacker);
            StatusService.ApplyEndOfTurnStatus(Defender);
        }

        public void SwitchAttackerDefender() => (_state.Attacker, _state.Defender) = (_state.Defender, _state.Attacker);

        public bool AttackerMovesFirst(int attackerPriority, int defenderPriority)
            => TurnResolver.AttackerMovesFirst(_state.Attacker, _state.Defender, attackerPriority, defenderPriority);

        public BattleSideState GetSide(BattleSide side) => side == BattleSide.Attacker ? AttackerSide : DefenderSide;

        public bool IsBattleOver => _state.Attacker.IsFainted || _state.Defender.IsFainted;
        public PokemonHelper.PokemonState? Winner => _state.Attacker.IsFainted ? _state.Defender : _state.Defender.IsFainted ? _state.Attacker : null;
    }
}
