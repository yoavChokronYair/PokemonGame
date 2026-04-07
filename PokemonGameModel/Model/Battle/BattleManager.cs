
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
        private readonly PokemonTeam _playerTeam;
        private readonly PokemonTeam _botTeam;
        private readonly BattleState _state;

        public BattlePhase Phase { get; private set; } = BattlePhase.AwaitingPlayerAction;

        private IMove? _pendingPlayerMove;
        private IMove? _pendingBotMove;

        public PokemonState PlayerActive => _playerTeam.Active;
        public PokemonState BotActive => _botTeam.Active;
        public IReadOnlyList<string> BattleLog => _state.Logger.BattleLog;
        public IReadOnlyList<BattleLogEntry> BattleLogEntries => _state.Logger.Entries;
        public bool IsBattleOver => Phase == BattlePhase.BattleOver;
        public PokemonTeam? Winner { get; private set; }
        public PokemonTeam? Loser { get; private set; }

        public BattleManager(PokemonTeam playerTeam, PokemonTeam botTeam)
        {
            _playerTeam = playerTeam;
            _botTeam = botTeam;

            _state = new BattleState(playerTeam.Active, botTeam.Active);
            _state.Logger.LogSetup($"Enemy sent out {_botTeam.Active.Name}!");
            _state.Logger.LogSetup($"Go! {_playerTeam.Active.Name}!");

            new StartBattle().Run(_state);
        }

        public bool RunTurn(int playerMoveIndex, bool botDecides = true)
        {
            if (Phase != BattlePhase.AwaitingPlayerAction)
                return false;

            _pendingPlayerMove = GetMoveOrFallback(PlayerActive, playerMoveIndex);
            _pendingBotMove = botDecides ? PickBotMove(BotActive) : GetMoveOrFallback(BotActive, 0);

            Phase = BattlePhase.ResolvingTurn;

            _state.UpdateActivePair(PlayerActive, BotActive);

            new BeginTurn().Run(_state);
            new ResolveTurn(_pendingPlayerMove, _pendingBotMove, PlayerActive, BotActive).Run(_state);
            new EndTurn().Run(_state);
            new HandleFaints(PlayerActive, BotActive).Run(_state);

            HandlePostTurnFaints();
            return true;
        }

        public bool PlayerSwitch(int slotIndex)
        {
            if (Phase != BattlePhase.AwaitingPlayerSwitch)
                return false;

            if (!_playerTeam.SwitchTo(slotIndex))
                return false;

            _state.UpdateActivePair(PlayerActive, BotActive);
            new SwitchIn(PlayerActive).Run(_state);

            Phase = BattlePhase.AwaitingPlayerAction;
            return true;
        }

        public IReadOnlyList<int> GetPlayerSwitchOptions() => _playerTeam.GetSwitchableIndices();

        // ── Post-turn routing only — no battle logic ──────────────────────────────
        private void HandlePostTurnFaints()
        {
            if (_playerTeam.IsDefeated) { SetBattleOver(_botTeam, _playerTeam); return; }
            if (_botTeam.IsDefeated) { SetBattleOver(_playerTeam, _botTeam); return; }

            if (BotActive.IsFainted)
            {
                _botTeam.SwitchToNextAvailable();
                new SwitchIn(BotActive).Run(_state);
                _state.UpdateActivePair(PlayerActive, BotActive);
            }

            Phase = PlayerActive.IsFainted
                ? BattlePhase.AwaitingPlayerSwitch
                : BattlePhase.AwaitingPlayerAction;
        }

        private void SetBattleOver(PokemonTeam winner, PokemonTeam loser)
        {
            Winner = winner;
            Loser = loser;
            Phase = BattlePhase.BattleOver;
            _state.Logger.Log(ReferenceEquals(winner, _playerTeam) ? "Player wins!" : "Bot wins!");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private static IMove GetMoveOrFallback(PokemonState pokemon, int index)
        {
            if (pokemon.Moves.Count == 0)
                throw new InvalidOperationException($"{pokemon.Name} has no moves.");

            return pokemon.Moves[MathHelper.Clamp(index, 0, pokemon.Moves.Count - 1)];
        }
        private static IMove PickBotMove(PokemonState bot) => bot.Moves[0];
    }
}
