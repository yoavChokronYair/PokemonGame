using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Battle;

namespace PokemonGame.Model.Domain.Battle
{
    // Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
    // Layer: Domain — processed battle state; no SQLite, no UI.
    // OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
    // Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
    public class BattleState
    {
        // ── Core State ────────────────────────────────────────────────────────
        public PokemonState Attacker { get; set; }
        public PokemonState Defender { get; set; }
        public BattleSideState AttackerSide { get; } = new();
        public BattleSideState DefenderSide { get; } = new();
        public IMove? LastUsedMove { get; set; }
        public PokemonType? ActiveTypeOverride { get; set; } = null;
        public int TurnNumber { get; set; } = 0;
        public int LastDamageDealt { get; set; } = 0;
        public bool LastMoveHit { get; set; }
        public bool LastMoveWasCritical { get; set; }
        public bool LastMoveMadeContact { get; set; }
        public int LastDamageTaken { get; set; }

        // ── Services ──────────────────────────────────────────────────────────
        public BattleWeatherService WeatherService { get; }
        public BattleStatusService StatusService { get; }
        public BattleTerrainService TerrainService { get; }
        public BattleLogger Logger { get; } = new();
        public BattleTurnResolver TurnResolver { get; }
        public BattleFieldState Field { get; } = new();

        public bool IsGravityActive => Field.IsGravityActive;

        public BattleState(PokemonState attacker, PokemonState defender)
        {
            Attacker = attacker;
            Defender = defender;
            WeatherService = new BattleWeatherService(this, Logger);
            StatusService = new BattleStatusService(Logger);
            TurnResolver = new BattleTurnResolver();
            TerrainService = new BattleTerrainService(this, Logger);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        public void RegisterMove(IMove move) => LastUsedMove = move;

        public void UpdateActivePair(PokemonState attacker, PokemonState defender)
        {
            Attacker = attacker;
            Defender = defender;
        }

        public bool AttackerMovesFirst(int attackerPriority, int defenderPriority)
            => TurnResolver.AttackerMovesFirst(Attacker, Defender, attackerPriority, defenderPriority);

        public BattleSideState GetSide(BattleSide side)
            => side == BattleSide.Attacker ? AttackerSide : DefenderSide;
        public void IncrementTurn() => TurnNumber++;
        public void ResetDamage()
        {
            LastDamageDealt = 0;
            LastDamageTaken = 0;
            LastMoveHit = false;
            LastMoveWasCritical = false;
            LastMoveMadeContact = false;
        }
        public bool IsBattleOver => Attacker.IsFainted || Defender.IsFainted;
    }
}