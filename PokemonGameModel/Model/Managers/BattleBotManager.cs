using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.Managers
{
    public class BotAction
    {
        public enum ActionType { Attack, Heal, Switch }

        public ActionType Type { get; }
        public IMove? Move { get; }       // set when Type == Attack
        public int? SwitchSlot { get; }   // set when Type == Switch

        private BotAction(ActionType type, IMove? move = null, int? switchSlot = null)
        {
            Type = type;
            Move = move;
            SwitchSlot = switchSlot;
        }

        public static BotAction Attack(IMove move) => new(ActionType.Attack, move: move);
        public static BotAction Heal() => new(ActionType.Heal);
        public static BotAction Switch(int slot) => new(ActionType.Switch, switchSlot: slot);
    }

    internal class BattleBotManager
    {
        // ── Tuning constants ────────────────────────────────────────────────────

        /// <summary>Medium/Hard: use a healing item when HP drops at or below this fraction.</summary>
        private const float HealHpThreshold = 0.40f;

        /// <summary>Hard only: proactively switch out when HP drops at or below this fraction.</summary>
        private const float SwitchHpThreshold = 0.25f;

        // ── State ───────────────────────────────────────────────────────────────

        private readonly BotLevel _level;
        private readonly PokemonTeam _bot;
        

        public BattleBotManager(BotLevel level, PokemonTeam bot, Random? rng = null)
        {
            _level = level;
            _bot = bot;
        }

        public BotAction PickAction()
        {
            // A fainted active Pokémon is always handled first regardless of level.
            if (_bot.Active.IsFainted)
                return ForcedSwitch();

            return _level switch
            {
                BotLevel.Easy => EasyAction(),
                BotLevel.Medium => MediumAction(),
                BotLevel.Hard => HardAction(),
                _ => EasyAction()
            };
        }

      
        private BotAction EasyAction()
        {
            var usable = UsableMoves();
            return BotAction.Attack(usable[RandomHelper.Next(0,usable.Count)]);
        }

        /// <summary>
        /// Medium: heal when HP is critically low, otherwise use the
        /// highest-power usable move.
        /// </summary>
        private BotAction MediumAction()
        {
            if (ShouldHeal())
                return BotAction.Heal();

            return BotAction.Attack(BestMove());
        }

        /// <summary>
        /// Hard: proactively switches to the healthiest bench Pokémon when
        /// badly hurt, heals when moderately low, otherwise uses the best move.
        /// </summary>
        private BotAction HardAction()
        {
            // Critically low HP — switch out if a healthier ally exists
            if (HpFraction(_bot.Active) <= SwitchHpThreshold)
            {
                int? slot = HealthiestSwitchSlot();
                if (slot.HasValue)
                    return BotAction.Switch(slot.Value);
            }

            // Moderately low HP — consume a healing item
            if (ShouldHeal())
                return BotAction.Heal();

            return BotAction.Attack(BestMove());
        }

        // ── Forced switch (active fainted) ──────────────────────────────────────

        private BotAction ForcedSwitch()
        {
            // Hard: pick the healthiest available Pokémon
            if (_level == BotLevel.Hard)
            {
                int? slot = HealthiestSwitchSlot();
                if (slot.HasValue)
                    return BotAction.Switch(slot.Value);
            }

            // Easy / Medium: pick the first available Pokémon
            var indices = _bot.GetSwitchableIndices();
            if (indices.Count == 0)
                throw new InvalidOperationException(
                    "Bot has no switchable Pokémon but was asked to switch.");

            return BotAction.Switch(indices[0]);
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        /// <summary>Moves that still have PP remaining.</summary>
        private IReadOnlyList<IMove> UsableMoves()
        {
            var moves = _bot.Active.Moves.Where(m => ((MoveState)(m)).PP > 0).ToList();

            // Fallback: if every move is out of PP, use Struggle (index 0).
            // Adjust this to however your engine handles the no-PP edge case.
            if (moves.Count == 0)
                moves.Add(_bot.Active.Moves[0]);

            return moves;
        }

        /// <summary>The usable move with the highest base power.</summary>
        private IMove BestMove() =>
            UsableMoves().OrderByDescending(m => m).First();

        /// <summary>
        /// True when the active Pokémon is at or below the heal threshold
        /// AND the bot still has a healing item to use.
        /// Replace HasHealItem / UseHealItem with your actual item API.
        /// </summary>
        private bool ShouldHeal() =>
            HpFraction(_bot.Active) <= HealHpThreshold && _bot.Active.HeldItem is HeldItemState;

        /// <summary>HP remaining as a 0–1 fraction.</summary>
        private static float HpFraction(PokemonState p) =>
            p.MaxHP > 0 ? (float)p.CurrentHP / p.MaxHP : 0f;

        /// <summary>
        /// Returns the slot index of the bench Pokémon with the highest current HP,
        /// or null if no switch is legal right now.
        /// </summary>
        private int? HealthiestSwitchSlot()
        {
            var candidates = _bot.GetSwitchableIndices();
            if (candidates.Count == 0)
                return null;

            // GetSwitchableIndices already excludes the active slot and fainted Pokémon.
            // We need the PokemonState for each candidate — expose an indexer on PokemonTeam
            // or keep a reference to the underlying array if your architecture allows it.
            return candidates
                .OrderByDescending(i => _bot.GetPokemonAt(i).CurrentHP)
                .First();
        }
    }

}
