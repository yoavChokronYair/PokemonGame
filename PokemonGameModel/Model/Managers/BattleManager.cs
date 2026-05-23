// PokemonGameModel/Model/Managers/BattleManager.cs
//
// CHANGES vs the file you uploaded:
//   1. Added RunTurnPvP(int playerIndex, int opponentIndex) — used by the
//      server's ServerBattleSession.RunPvPTurn() so both human players'
//      chosen move indices are used instead of running the bot AI.
//      Everything else is identical to the file you already have.

using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Battle;

namespace PokemonGame.Model.Model.Managers
{
    public class BattleManager
    {
        private readonly PokemonTeam _playerTeam;
        private readonly PokemonTeam _botTeam;
        private readonly BattleState _state;
        private readonly BattleBotManager _botManager;
        public bool HasBotFainted { get; private set; } = false;
        public bool HasTrainerFainted{ get; private set; } = false;
        public PokemonState PlayerActive => _playerTeam.Active;
        public PokemonState BotActive => _botTeam.Active;

        public PokemonTeam PlayerTeam => _playerTeam;
        public PokemonTeam BotTeam => _botTeam;

        public PokemonTeam? Winner { get; private set; }
        public readonly BattleLogger logger;
        public PokemonTeam? Loser { get; private set; }

        public BattleManager(PokemonTeam playerTeam, PokemonTeam botTeam, BotLevel botLevel)
        {
            _playerTeam = playerTeam;
            _botTeam = botTeam;
            _state = new BattleState(playerTeam.Active, botTeam.Active);
            _botManager = new BattleBotManager(botLevel, _botTeam);
            logger = _state.Logger;
            StartBattle();
        }

        private void StartBattle()
        {
            new StartBattle().Run(_state);
            _state.Logger.LogSetup($"Enemy sent out {_botTeam.Active.Name}!");
            _state.Logger.LogSetup($"Go! {_playerTeam.Active.Name}!");
        }

        public void ForceWinner(PokemonTeam winner)
        {
            PokemonTeam loser = (winner == _playerTeam) ? _botTeam : _playerTeam;
            Winner = winner;
            Loser = loser;
            _state.Logger.LogBattleEnd("The battle ended by declaration (Forfeit).");
            _state.Logger.LogBattleEnd($"Winner: {(Winner == _playerTeam ? "Player" : "Opponent")}");
        }

        private void EndBattle(PokemonTeam winner, PokemonTeam loser)
        {
            Winner = winner;
            Loser = loser;
            _state.Logger.LogBattleEnd("The battle is over.");
            _state.Logger.LogBattleEnd($"{Winner?.Active.Name} wins with {Winner?.GetAlivePokemonCount()} Pokémon left!");
        }

        // ── Existing offline turn — bot AI picks the opponent move ────────────
        public bool RunTurn(int playerIndex, BattleAction playerAction = BattleAction.Move)
        {
            HasBotFainted = false;
            HasTrainerFainted = false;
            BotAction botAction = _botManager.PickAction();

            IMove? pendingPlayerMove = null;
            IMove? pendingBotMove =
                botAction.Type == BotAction.ActionType.Attack
                    ? botAction.Move
                    : null;

            // PLAYER SWITCH
            if (playerAction == BattleAction.Switch)
            {
                if (!_playerTeam.SwitchTo(playerIndex))
                    return false;

                _state.UpdateActivePair(PlayerActive, BotActive);
                new BeginTurn().Run(_state);
                new SwitchIn(PlayerActive).Run(_state);

                if (pendingBotMove != null && !BotActive.IsFainted)
                {
                    new MoveExecution(
                        pendingBotMove,
                        BotActive,
                        PlayerActive).Run(_state);
                }

                new HandleFaints(PlayerActive, BotActive).Run(_state);
                new EndTurn().Run(_state);
                HandlePostTurnFaints();
                return true;
            }

            // ITEM
            if (playerAction == BattleAction.Item)
                return playerUseItem(playerIndex);

            // NORMAL MOVE
            pendingPlayerMove =
                PlayerActive.Moves[
                    MathHelper.Clamp(playerIndex, 0, PlayerActive.Moves.Count - 1)];

            if (botAction.Type == BotAction.ActionType.Switch)
            {
                _botTeam.SwitchTo(botAction.SwitchSlot!.Value);
                new SwitchIn(BotActive).Run(_state);
                _state.UpdateActivePair(PlayerActive, BotActive);
            }

            if (botAction.Type == BotAction.ActionType.Heal)
                BotActive.UseHealItem();

            _state.UpdateActivePair(PlayerActive, BotActive);
            new BeginTurn().Run(_state);

            new ResolveTurn(
                pendingPlayerMove,
                pendingBotMove,
                PlayerActive,
                BotActive).Run(_state);

            new HandleFaints(PlayerActive, BotActive).Run(_state);
            new EndTurn().Run(_state);
            HandlePostTurnFaints();
            return true;
        }
        public void LogSwitchPromptAfterBotFaint()
        {
            logger.LogSwitch($"Enemy is about to send out {BotActive.Name}.");
            logger.LogSwitch("Will you switch Pokémon?");
        }

        public bool FreeSwitchPlayer(int slotIndex)
        {
            if (Winner != null)
                return false;

            if (!_playerTeam.SwitchTo(slotIndex))
                return false;

            _state.UpdateActivePair(PlayerActive, BotActive);
            new SwitchIn(PlayerActive).Run(_state);

            return true;
        }
        // ── NEW: PvP turn — both move indices come from human players ─────────
        // Used by ServerBattleSession.RunPvPTurn() so the server never consults
        // the bot AI during an online match.  The "player" side maps to Player 1
        // and the "bot/opponent" side maps to Player 2 (same team layout as
        // the existing BattleManager; only the source of the move index differs).
        public bool RunTurnPvP(int playerIndex, int opponentIndex,
                               BattleAction playerAction = BattleAction.Move,
                               BattleAction opponentAction = BattleAction.Move)
        {
            IMove? pendingPlayerMove = null;
            IMove? pendingOpponentMove = null;

            // ── PLAYER SWITCH ─────────────────────────────────────────────────
            if (playerAction == BattleAction.Switch)
            {
                if (!_playerTeam.SwitchTo(playerIndex))
                    return false;

                _state.UpdateActivePair(PlayerActive, BotActive);
                new BeginTurn().Run(_state);
                new SwitchIn(PlayerActive).Run(_state);

                // Opponent still attacks after player switches
                if (opponentAction == BattleAction.Move)
                {
                    pendingOpponentMove = BotActive.Moves[
                        MathHelper.Clamp(opponentIndex, 0, BotActive.Moves.Count - 1)];

                    if (pendingOpponentMove != null && !BotActive.IsFainted)
                    {
                        new MoveExecution(
                            pendingOpponentMove,
                            BotActive,
                            PlayerActive).Run(_state);
                    }
                }

                new HandleFaints(PlayerActive, BotActive).Run(_state);
                new EndTurn().Run(_state);
                HandlePostTurnFaints();
                return true;
            }

            // ── OPPONENT SWITCH ───────────────────────────────────────────────
            if (opponentAction == BattleAction.Switch)
            {
                if (!_botTeam.SwitchTo(opponentIndex))
                    return false;

                _state.UpdateActivePair(PlayerActive, BotActive);
                new SwitchIn(BotActive).Run(_state);

                // Player still attacks after opponent switches
                if (playerAction == BattleAction.Move)
                {
                    pendingPlayerMove = PlayerActive.Moves[
                        MathHelper.Clamp(playerIndex, 0, PlayerActive.Moves.Count - 1)];

                    if (pendingPlayerMove != null && !PlayerActive.IsFainted)
                    {
                        new MoveExecution(
                            pendingPlayerMove,
                            PlayerActive,
                            BotActive).Run(_state);
                    }
                }

                new HandleFaints(PlayerActive, BotActive).Run(_state);
                new EndTurn().Run(_state);
                HandlePostTurnFaints();
                return true;
            }

            // ── BOTH USE MOVES ────────────────────────────────────────────────
            pendingPlayerMove = PlayerActive.Moves[
                MathHelper.Clamp(playerIndex, 0, PlayerActive.Moves.Count - 1)];
            pendingOpponentMove = BotActive.Moves[
                MathHelper.Clamp(opponentIndex, 0, BotActive.Moves.Count - 1)];

            _state.UpdateActivePair(PlayerActive, BotActive);
            new BeginTurn().Run(_state);

            new ResolveTurn(
                pendingPlayerMove,
                pendingOpponentMove,
                PlayerActive,
                BotActive).Run(_state);

            new HandleFaints(PlayerActive, BotActive).Run(_state);
            new EndTurn().Run(_state);
            HandlePostTurnFaints();
            return true;
        }
        public bool playerUseItem(int itemIndex)
        {
            _state.Logger.LogSetup("Item usage not implemented yet.");
            return false;
        }

        public void BeginTurn()
        {
            new BeginTurn().Run(_state);
        }

        private void HandlePostTurnFaints()
        {
            if (_playerTeam.IsDefeated)
            {
                EndBattle(_botTeam, _playerTeam);

                return;
            }
            if (_botTeam.IsDefeated)
            {
                EndBattle(_playerTeam, _botTeam);
                return;
            }
            if (BotActive.IsFainted)
            {
                HasBotFainted = true;
                BotAction forcedSwitch = _botManager.PickAction();
                _botTeam.SwitchTo(forcedSwitch.SwitchSlot!.Value);
                new SwitchIn(BotActive).Run(_state);
                _state.UpdateActivePair(PlayerActive, BotActive);
            }
            if (PlayerActive.IsFainted)
            {
                HasTrainerFainted = true;
                _playerTeam.SwitchToNextAvailable();
                new SwitchIn(PlayerActive).Run(_state);
                _state.UpdateActivePair(PlayerActive, BotActive);
            }
        }
    }
}