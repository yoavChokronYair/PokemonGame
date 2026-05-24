// PokemonGameModel/Model/Managers/BattleManager.cs
//
// CHANGES vs the file you uploaded:
//   1. Added RunTurnPvP(int playerIndex, int opponentIndex) — used by the
//      server's ServerBattleSession.RunPvPTurn() so both human players'
//      chosen move indices are used instead of running the bot AI.
//      Everything else is identical to the file you already have.

using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Battle;

namespace PokemonGame.Model.Model.Managers
{
    public class CatchAttemptResult
    {
        public bool Caught { get; set; }
        public int ShakeCount { get; set; }
        public PokeBallType BallType { get; set; }
    }
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
        private readonly HashSet<PokemonState> _eligibleRewardRecipients = new();

        public PokemonTeam PlayerTeam => _playerTeam;
        public PokemonTeam BotTeam => _botTeam;

        public PokemonTeam? Winner { get; private set; }
        public readonly BattleLogger logger;
        public PokemonTeam? Loser { get; private set; }
        private int fleeAttempts = 0;

        public BattleManager(PokemonTeam playerTeam, PokemonTeam botTeam, BotLevel botLevel)
        {
            _playerTeam = playerTeam;
            _botTeam = botTeam;
            _state = new BattleState(playerTeam.Active, botTeam.Active);
            _botManager = new BattleBotManager(botLevel, _botTeam);
            logger = _state.Logger;

            StartBattle();

            RegisterRewardParticipant(PlayerActive);
        }
        private void RegisterRewardParticipant(PokemonState pokemon)
        {
            if (pokemon == null)
                return;

            if (pokemon.IsFainted)
                return;

            if (_playerTeam.Members.Contains(pokemon))
                _eligibleRewardRecipients.Add(pokemon);
        }
        public IReadOnlyCollection<PokemonState> EligibleRewardRecipients =>
            _eligibleRewardRecipients;
        private void StartBattle()
        {
            new StartBattle().Run(_state);
            _state.Logger.LogSetup($"Enemy sent out {_botTeam.Active.Name}!");
            _state.Logger.LogSetup($"Go! {_playerTeam.Active.Name}!");
        }
        public CatchAttemptResult TryThrowBall(
            WildPokemonDomain wildPokemon,
            PokeballState ball)
        {
            if (wildPokemon == null)
                throw new ArgumentNullException(nameof(wildPokemon));

            if (ball == null)
                throw new ArgumentNullException(nameof(ball));

            HasBotFainted = false;
            HasTrainerFainted = false;

            _state.Logger.Log($"You threw a {ball.Name}!");

            var roll = RNGHelper.RollCatch(
                wildPokemon,
                ball,
                _state);

            if (roll.Caught)
            {
                _state.Logger.Log("1...");
                _state.Logger.Log("2...");
                _state.Logger.Log("3...");
                _state.Logger.Log($"{wildPokemon.pokemonState.Name} was caught!");

                // BUG-105:
                // Apply special ball caught effect.
                ball.ApplyCaughtEffect(_state);

                return new CatchAttemptResult
                {
                    Caught = true,
                    ShakeCount = roll.ShakeCount,
                    BallType = ball.BallType
                };
            }

            foreach (string message in BuildBreakFreeMessages(roll.ShakeCount))
            {
                _state.Logger.Log(message);
            }

            // BUG-102:
            // Failed catch consumes the player's action,
            // then the wild Pokémon retaliates normally.
            RunWildRetaliationTurn();

            return new CatchAttemptResult
            {
                Caught = false,
                ShakeCount = roll.ShakeCount,
                BallType = ball.BallType
            };
        }
        public bool RunWildRetaliationTurn()
        {
            if (Winner != null)
                return false;

            BotAction botAction = _botManager.PickAction();

            IMove? pendingBotMove =
                botAction.Type == BotAction.ActionType.Attack
                    ? botAction.Move
                    : null;

            if (pendingBotMove == null)
                return false;

            _state.UpdateActivePair(PlayerActive, BotActive);

            new BeginTurn().Run(_state);

            if (!BotActive.IsFainted)
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
        private static string[] BuildBreakFreeMessages(int shakeCount)
        {
            return shakeCount switch
            {
                0 => new[]
                {
                    "Oh no! The Pokémon broke free!"
                },

                        1 => new[]
                        {
                    "1...",
                    "Aww! It appeared to be caught!"
                },

                        2 => new[]
                        {
                    "1...",
                    "2...",
                    "Aargh! Almost had it!"
                },

                        3 => new[]
                        {
                    "1...",
                    "2...",
                    "3...",
                    "Shoot! It was so close, too!"
                },

                        _ => new[]
                        {
                    "Oh no! The Pokémon broke free!"
                }
            };
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
        public bool RunTurn(int playerIndex, BattleActionType playerAction = BattleActionType.Move)
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
            if (playerAction == BattleActionType.Switch)
            {
                if (!_playerTeam.SwitchTo(playerIndex))
                    return false;

                _state.UpdateActivePair(PlayerActive, BotActive);
                RegisterRewardParticipant(PlayerActive);
                new BeginTurn().Run(_state);
                new SwitchIn(PlayerActive, _state.AttackerSide).Run(_state);

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
            if (playerAction == BattleActionType.Item)
                return playerUseItem(playerIndex);
            RegisterRewardParticipant(PlayerActive);
            // NORMAL MOVE
            pendingPlayerMove =
                PlayerActive.Moves[
                    MathHelper.Clamp(playerIndex, 0, PlayerActive.Moves.Count - 1)];

            if (botAction.Type == BotAction.ActionType.Switch)
            {
                _botTeam.SwitchTo(botAction.SwitchSlot!.Value);
                new SwitchIn(BotActive, _state.DefenderSide).Run(_state);
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
        public bool TryFlee()
        {
            fleeAttempts++;

            bool canFlee = RNGHelper.CanEscapeWildEncounter(
                PlayerActive,
                BotActive,
                fleeAttempts);

            if (canFlee)
            {
                _state.Logger.LogBattleEnd("Got away safely!");
            }
            else
            {
                _state.Logger.LogBattleEnd("Couldn't escape!");
            }

            return canFlee;
        }
        public void LogSwitchPromptAfterBotFaint()
        {
            logger.LogSwitch($"Enemy is about to send out {BotActive.Name}.");
            logger.LogSwitch("Will you switch Pokémon?");
        }
        private bool CanSwitchOut(PokemonState pokemon)
        {
            if (pokemon.HasVolatileStatus(VolatileStatus.Trapped))
            {
                _state.Logger.LogStatus($"{pokemon.Name} can't escape!");
                return false;
            }

            if (pokemon.Ability is AbilityState ability &&
                ability.Name.Equals("Shadow Tag", StringComparison.OrdinalIgnoreCase))
            {
                // This is only an example. Real Shadow Tag belongs on the opponent,
                // not the switching Pokémon.
                return false;
            }

            return true;
        }
        public bool FreeSwitchPlayer(int slotIndex)
        {
            if (Winner != null)
                return false;

            if (!_playerTeam.SwitchTo(slotIndex))
                return false;

            _state.UpdateActivePair(PlayerActive, BotActive);
            RegisterRewardParticipant(PlayerActive);
            new SwitchIn(PlayerActive, _state.AttackerSide).Run(_state);

            return true;
        }
        // ── NEW: PvP turn — both move indices come from human players ─────────
        // Used by ServerBattleSession.RunPvPTurn() so the server never consults
        // the bot AI during an online match.  The "player" side maps to Player 1
        // and the "bot/opponent" side maps to Player 2 (same team layout as
        // the existing BattleManager; only the source of the move index differs).
        public bool RunTurnPvP(int playerIndex, int opponentIndex,
                               BattleActionType playerAction = BattleActionType.Move,
                               BattleActionType opponentAction = BattleActionType.Move)
        {
            IMove? pendingPlayerMove = null;
            IMove? pendingOpponentMove = null;

            // ── PLAYER SWITCH ─────────────────────────────────────────────────
            if (playerAction == BattleActionType.Switch)
            {
                if (!_playerTeam.SwitchTo(playerIndex) || !CanSwitchOut(PlayerActive))
                    return false;

                _state.UpdateActivePair(PlayerActive, BotActive);
                new BeginTurn().Run(_state);
                new SwitchIn(PlayerActive, _state.AttackerSide).Run(_state);

                // Opponent still attacks after player switches
                if (opponentAction == BattleActionType.Move)
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
            if (opponentAction == BattleActionType.Switch)
            {
                if (!_botTeam.SwitchTo(opponentIndex))
                    return false;

                _state.UpdateActivePair(PlayerActive, BotActive);
                new SwitchIn(BotActive, _state.DefenderSide).Run(_state);
                // Player still attacks after opponent switches
                if (playerAction == BattleActionType.Move)
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
                new SwitchIn(BotActive, _state.DefenderSide).Run(_state);
                _state.UpdateActivePair(PlayerActive, BotActive);
            }
            if (PlayerActive.IsFainted)
            {
                HasTrainerFainted = true;
                _playerTeam.SwitchToNextAvailable();
                new SwitchIn(PlayerActive, _state.AttackerSide).Run(_state);
                _state.UpdateActivePair(PlayerActive, BotActive);
            }
        }
    }
}