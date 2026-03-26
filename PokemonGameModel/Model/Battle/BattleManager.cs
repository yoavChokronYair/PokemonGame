
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Model.Model.Helper.MoveHelper;
using PokemonGame.Model.Model.Helper.PokemonHelper;

namespace PokemonGame.Model.Model.Battle
{
    /// <summary>
    /// Phase the UI is in — drives what input the player must supply next.
    /// </summary>
    public enum BattlePhase
    {
        /// <summary>Player picks a move (or decides to switch voluntarily).</summary>
        AwaitingPlayerAction,

        /// <summary>Player must pick a replacement after their active Pokémon fainted.</summary>
        AwaitingPlayerSwitch,

        /// <summary>Both sides have acted; results are being resolved.</summary>
        ResolvingTurn,

        /// <summary>Battle is finished — check Winner/Loser.</summary>
        BattleOver,
    }

    public class BattleManager
    {
        // ── Teams ──────────────────────────────────────────────────────────────
        private readonly PokemonTeam _playerTeam;
        private readonly PokemonTeam _botTeam;

        // ── Core battle state (single-active-pair wrapper) ─────────────────────
        private readonly BattleDomain _domain;
        private readonly BattleState _state;

        // ── Phase tracking ─────────────────────────────────────────────────────
        public BattlePhase Phase { get; private set; } = BattlePhase.AwaitingPlayerAction;

        // ── Pending moves (set before resolution) ─────────────────────────────
        private IMove? _pendingPlayerMove;
        private IMove? _pendingBotMove;

        // ── Public read-only surfaces ─────────────────────────────────────────
        public PokemonState PlayerActive => _playerTeam.Active;
        public PokemonState BotActive => _botTeam.Active;
        public IReadOnlyList<string> BattleLog => _state.Logger.BattleLog;
        public bool IsBattleOver => Phase == BattlePhase.BattleOver;

        /// <summary>Null until the battle ends.</summary>
        public PokemonTeam? Winner { get; private set; }
        public PokemonTeam? Loser { get; private set; }

        // ── Constructor ───────────────────────────────────────────────────────
        public BattleManager(PokemonTeam playerTeam, PokemonTeam botTeam)
        {
            _playerTeam = playerTeam;
            _botTeam = botTeam;

            _domain = new BattleDomain
            {
                Attacker = playerTeam.Active,
                Defender = botTeam.Active
            };

            _state = new BattleState(_domain);

            _state.Logger.Log($"Battle start! {_playerTeam.Active.Name} vs {_botTeam.Active.Name}");
        }

        // ── Primary API ────────────────────────────────────────────────────────

        /// <summary>
        /// Execute one full turn.
        /// playerMoveIndex — index into PlayerActive.Moves (ignored when botDecides == true for testing).
        /// botDecides — if true the bot AI picks its own move; if false the bot uses move index 0 (for tests).
        /// Returns false if the phase prevents acting (e.g., awaiting a switch).
        /// </summary>
        public bool RunTurn(int playerMoveIndex, bool botDecides = true)
        {
            if (Phase != BattlePhase.AwaitingPlayerAction)
                return false;

            // 1. Gather moves
            _pendingPlayerMove = GetMoveOrFallback(PlayerActive, playerMoveIndex);
            _pendingBotMove = botDecides
                ? PickBotMove(BotActive)
                : GetMoveOrFallback(BotActive, 0);

            Phase = BattlePhase.ResolvingTurn;

            // 2. Sync active Pokémon into BattleDomain before resolution
            SyncActivePokemon();

            // 3. Run the turn
            ExecuteTurn();

            // 4. Check end-of-turn faint conditions
            HandlePostTurnFaints();

            return true;
        }

        /// <summary>
        /// Called by the UI when the player has chosen a Pokémon to send in after a faint.
        /// Returns false if the slot is invalid.
        /// </summary>
        public bool PlayerSwitch(int slotIndex)
        {
            if (Phase != BattlePhase.AwaitingPlayerSwitch)
                return false;

            bool ok = _playerTeam.SwitchTo(slotIndex);
            if (!ok) return false;

            SyncActivePokemon();
            _state.Logger.Log($"Player sends out {PlayerActive.Name}!");

            // After the forced switch the bot might also need to act — but for now
            // we simply advance to the next awaiting-action phase.
            Phase = BattlePhase.AwaitingPlayerAction;
            return true;
        }

        /// <summary>Indices the player can legally switch to right now.</summary>
        public IReadOnlyList<int> GetPlayerSwitchOptions() => _playerTeam.GetSwitchableIndices();

        // ── Turn execution ─────────────────────────────────────────────────────

        private void ExecuteTurn()
        {
            _state.BeginTurn();

            int playerPriority = (_pendingPlayerMove as MoveState)?.Priority ?? 0;
            int botPriority = (_pendingBotMove as MoveState)?.Priority ?? 0;

            bool playerFirst = _state.AttackerMovesFirst(playerPriority, botPriority);

            if (playerFirst)
            {
                ExecuteMove(_pendingPlayerMove!, PlayerActive, BotActive);
                if (!BotActive.IsFainted)
                    ExecuteMove(_pendingBotMove!, BotActive, PlayerActive);
            }
            else
            {
                ExecuteMove(_pendingBotMove!, BotActive, PlayerActive);
                if (!PlayerActive.IsFainted)
                    ExecuteMove(_pendingPlayerMove!, PlayerActive, BotActive);
            }

            _state.EndTurn(PlayerActive,BotActive);
        }

        private void ExecuteMove(IMove move, PokemonState user, PokemonState target)
        {
            if (user.IsFainted) return;

            _state.RegisterMove(move);

            bool userIsPlayer = ReferenceEquals(user, PlayerActive);

            // Always set Attacker = actual user, Defender = actual target
            _domain.Attacker = user;
            _domain.Defender = target;

            // If the bot is acting, flip the state's perspective so move
            // logic that reads _state.Attacker/Defender sees the right sides
            if (!userIsPlayer)
                _state.SwitchAttackerDefender();

            move.Execute(_state);

            // Restore to player-perspective after bot's move
            if (!userIsPlayer)
                _state.SwitchAttackerDefender();
        }

        // ── Post-turn faint handling ───────────────────────────────────────────

        private void HandlePostTurnFaints()
        {
            bool playerFainted = PlayerActive.IsFainted;
            bool botFainted = BotActive.IsFainted;

            if (playerFainted)
            {
                _state.Logger.Log($"{PlayerActive.Name} fainted!");
            }

            if (botFainted)
            {
                _state.Logger.Log($"{BotActive.Name} fainted!");
            }

            // Check team defeat first
            if (_playerTeam.IsDefeated)
            {
                SetBattleOver(winner: _botTeam, loser: _playerTeam);
                return;
            }

            if (_botTeam.IsDefeated)
            {
                SetBattleOver(winner: _playerTeam, loser: _botTeam);
                return;
            }

            // Bot auto-switches if its active fainted
            if (botFainted)
            {
                _botTeam.SwitchToNextAvailable();
                _state.Logger.Log($"Bot sends out {BotActive.Name}!");
                SyncActivePokemon();
            }

            // Player must manually choose
            if (playerFainted)
            {
                Phase = BattlePhase.AwaitingPlayerSwitch;
                return;
            }

            Phase = BattlePhase.AwaitingPlayerAction;
        }

        private void SetBattleOver(PokemonTeam winner, PokemonTeam loser)
        {
            Winner = winner;
            Loser = loser;
            Phase = BattlePhase.BattleOver;
            _state.Logger.Log(ReferenceEquals(winner, _playerTeam)
                ? "Player wins!"
                : "Bot wins!");
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Keep BattleDomain.Attacker/Defender in sync with the live active slots.
        /// Call after any switch.
        /// </summary>
        private void SyncActivePokemon()
        {
            _domain.Attacker = _playerTeam.Active;
            _domain.Defender = _botTeam.Active;
        }

        private static IMove GetMoveOrFallback(PokemonState pokemon, int index)
        {
            if (pokemon.Moves.Count == 0)
                throw new InvalidOperationException($"{pokemon.Name} has no moves.");

            int clamped = MathHelper.Clamp(index, 0, pokemon.Moves.Count - 1);
            return pokemon.Moves[clamped];
        }

        /// <summary>
        /// Minimal bot AI: picks the first move for now.
        /// Swap this out for type-effectiveness scoring, PP tracking, etc. later.
        /// </summary>
        private static IMove PickBotMove(PokemonState bot)
        {
            return bot.Moves[0];
        }
    }
}
