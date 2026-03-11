using PokemonGame.Enums.Battle;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums.Battle;
using PokemonGame.Model.Interface.Move;
using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    internal class BattleState
    {
        private readonly BattleDomain state;
        public BattleWeatherService WeatherService { get; }
        public BattleStatusService StatusService { get; }
        public BattleLogger Logger { get; } = new();
        public BattleTurnResolver TurnResolver { get; }

        public BattleState(BattleDomain state)
        {
            this.state = state;
            WeatherService = new BattleWeatherService(this, Logger);
            StatusService = new BattleStatusService(Logger);
            TurnResolver = new BattleTurnResolver();
        }

        // Data accessors
        public PokemonDomain Attacker => state.Attacker;
        public PokemonDomain Defender => state.Defender;
        public BattleSideState AttackerSide => state.AttackerSide;
        public BattleSideState DefenderSide => state.DefenderSide;
        public IMove? LastUsedMove => state.LastUsedMove;
        public PokemonType? ActiveTypeOverride { get => state.ActiveTypeOverride; set => state.ActiveTypeOverride = value; }
        public int TurnNumber => state.TurnNumber;
        public int LastDamageDealt { get => state.LastDamageDealt; set => state.LastDamageDealt = value; }

        // Logic
        public void RegisterMove(IMove move) => state.LastUsedMove = move;

        public void BeginTurn()
        {
            state.TurnNumber++;
            state.LastDamageDealt = 0;
            Logger.Log($"--- Turn {state.TurnNumber} ---");
        }

        public void EndTurn()
        {
            WeatherService.TickWeather();
            AttackerSide.Tick();
            DefenderSide.Tick();
            StatusService.ApplyEndOfTurnStatus(Attacker);
            StatusService.ApplyEndOfTurnStatus(Defender);
        }

        public void SwitchAttackerDefender() => (state.Attacker, state.Defender) = (state.Defender, state.Attacker);

        public bool AttackerMovesFirst(int attackerPriority, int defenderPriority)
            => TurnResolver.AttackerMovesFirst(state.Attacker, state.Defender, attackerPriority, defenderPriority);

        public BattleSideState GetSide(BattleSide side) => side == BattleSide.Attacker ? AttackerSide : DefenderSide;

        public bool IsBattleOver => state.Attacker.IsFainted || state.Defender.IsFainted;
        public PokemonDomain? Winner => state.Attacker.IsFainted ? state.Defender : state.Defender.IsFainted ? state.Attacker : null;
    }
}
