// PokemonGameModel/Model/Managers/BattleManager.cs
// CHANGE: Added two public properties PlayerTeam and BotTeam (lines marked NEW).
// Everything else is identical to your existing file.

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

        public PokemonState PlayerActive => _playerTeam.Active;
        public PokemonState BotActive => _botTeam.Active;

        // ── NEW: lets BattleRoom call GetSwitchableIndices() on each team ────
        public PokemonTeam PlayerTeam => _playerTeam;
        public PokemonTeam BotTeam => _botTeam;

        public PokemonTeam? Winner { get; private set; }
        public BattleLogger logger;
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
            // Determine the loser based on who is being forced as the winner
            PokemonTeam loser = (winner == _playerTeam) ? _botTeam : _playerTeam;

            Winner = winner;
            Loser = loser;

            // Log the end of the battle explicitly for the forfeit scenario
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

        public bool RunTurn(int playerIndex, BattleAction playerAction = BattleAction.Move)
        {
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

                // enemy still attacks after switch
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
                    MathHelper.Clamp(
                        playerIndex,
                        0,
                        PlayerActive.Moves.Count - 1)];

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

        private bool PlayerSwitch(int slotIndex)
        {
            if (!_playerTeam.SwitchTo(slotIndex))
                return false;

            _state.UpdateActivePair(PlayerActive, BotActive);
            new SwitchIn(PlayerActive).Run(_state);
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
                BotAction forcedSwitch = _botManager.PickAction();
                _botTeam.SwitchTo(forcedSwitch.SwitchSlot!.Value);
                new SwitchIn(BotActive).Run(_state);
                _state.UpdateActivePair(PlayerActive, BotActive);
            }
            if (PlayerActive.IsFainted)
            {
                _playerTeam.SwitchToNextAvailable();
                new SwitchIn(PlayerActive).Run(_state);
                _state.UpdateActivePair(PlayerActive, BotActive);
            }
        }
    }
}