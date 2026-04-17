using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Battle;

namespace PokemonGame.Model.Model.Managers
{
    //TODO:add a use item method
    public class BattleManager
    {
        private readonly PokemonTeam _playerTeam;
        private readonly PokemonTeam _botTeam;
        private readonly BattleState _state;
        private readonly BattleBotManager _botManager;
        public PokemonState PlayerActive => _playerTeam.Active;
        public PokemonState BotActive => _botTeam.Active;
        public PokemonTeam? Winner { get; private set; }
        public BattleLogger logger;
        public PokemonTeam? Loser { get; private set; }

        public BattleManager(PokemonTeam playerTeam, PokemonTeam botTeam,BotLevel botLevel)
        {
            _playerTeam = playerTeam;
            _botTeam = botTeam;
            _state = new BattleState(playerTeam.Active, botTeam.Active);
            _botManager = new BattleBotManager(botLevel,_botTeam);
            logger = _state.Logger;
            StartBattle();
        }
        private void StartBattle()
        {
            new StartBattle().Run(_state);
            _state.Logger.LogSetup($"Enemy sent out {_botTeam.Active.Name}!");
            _state.Logger.LogSetup($"Go! {_playerTeam.Active.Name}!");
        }
        private void EndBattle(PokemonTeam winner, PokemonTeam loser)
        {
            Winner = winner;
            Loser = loser;
            _state.Logger.LogBattleEnd("The battle is over.");
            _state.Logger.LogBattleEnd($"{Winner?.Active.Name} wins with {Winner?.GetAlivePokemonCount()} Pokémon left!");
        }
        
        public bool RunTurn(int playerIndex,BattleAction playerAction = BattleAction.Move)
        {
            BotAction botAction = _botManager.PickAction();
            IMove? pendingPlayerMove = null;
            IMove? pendingBotMove = botAction.Type == BotAction.ActionType.Attack
                            ? botAction.Move
                            : null; 
            if (playerAction == BattleAction.Switch)
            {
                return PlayerSwitch(playerIndex);
            }
            if(playerAction == BattleAction.Item)
            {
                return playerUseItem(playerIndex);
            }
            else
            {
                pendingPlayerMove = PlayerActive.Moves[MathHelper.Clamp(playerIndex, 0, PlayerActive.Moves.Count - 1)];
            }

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
            new ResolveTurn(pendingPlayerMove, pendingBotMove, PlayerActive, BotActive).Run(_state);
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
            // Item logic not implemented yet, return false to indicate failure
            _state.Logger.LogSetup("Item usage not implemented yet.");
            return false;
        }
        // ── Post-turn routing only — no battle logic ──────────────────────────────
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
