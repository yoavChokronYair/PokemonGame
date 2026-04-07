using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Model.Model.Helper.DesignPatterns;
using PokemonGame.Model.Model.Helper.PokemonHelper;

namespace PokemonGame.Model.Interface
{
    public interface IPhase
    {
        void Run(BattleState battleState);
    }
    // ── 1. Start Battle ──────────────────────────────────────────────
    // Triggered when the BattleManager is first initialized
    public class StartBattle : IPhase
    {
        public void Run(BattleState state)
        {
            state.Logger.Log("A battle has broken out!");
            // Trigger switch-in effects for the starting two
            new SwitchIn(state.Attacker).Run(state);
            new SwitchIn(state.Defender).Run(state);
        }
    }

    // ── 2. Switch In ─────────────────────────────────────────────────
    // Triggered at start of battle OR when a fainted Pokemon is replaced
    public class SwitchIn : IPhase
    {
        private readonly PokemonState _incoming;
        public SwitchIn(PokemonState incoming) => _incoming = incoming;

        public void Run(BattleState state)
        {
            state.Logger.Log($"{_incoming.Name} entered the battle!");

            // This is where Intimidate or Drizzle would go
            if (_incoming.Ability is OnSwitchIn switchAbility)
            {
                switchAbility.Apply(state);
            }
        }
    }

    // ── 3. Begin Turn ────────────────────────────────────────────────
    public class BeginTurn : IPhase
    {
        public void Run(BattleState state)
        {
            state.BeginTurn();
            ApplyIfTurnStart(state.Attacker.Ability, state);
            ApplyIfTurnStart(state.Defender.Ability, state);
        }

        private void ApplyIfTurnStart(IAbility? ability, BattleState state)
        {
            if (ability is OnTurnStart turnStartAbility)
                turnStartAbility.Apply(state);
        }
    }

    // ── 4. Move Execution ────────────────────────────────────────────
    public class MoveExecution : IPhase
    {
        private readonly IMove _move;
        private readonly PokemonState _user;
        private readonly PokemonState _target;

        public MoveExecution(IMove move, PokemonState user, PokemonState target)
        {
            _move = move;
            _user = user;
            _target = target;
        }

        public void Run(BattleState state)
        {
            if (_user.IsFainted) return;

            // Use the new method we just discussed
            state.UpdateActivePair(_user, _target);

            state.RegisterMove(_move);
            _move.Execute(state);

            // ── 5. Getting Hit (The "OnHit" Phase) ──
            // After damage, check for Static, Flame Body, etc.
            new ResolveOnHitEffect(_user, _target).Run(state);
        }
    }

    // ── 5. Resolve OnHit ─────────────────────────────────────────────
    public class ResolveOnHitEffect : IPhase
    {
        private readonly PokemonState _attacker;
        private readonly PokemonState _defender;

        public ResolveOnHitEffect(PokemonState attacker, PokemonState defender)
        {
            _attacker = attacker;
            _defender = defender;
        }

        public void Run(BattleState state)
        {
            // Defender triggers (e.g. Static)
            if (_defender.Ability is OnHit defAbility)
                defAbility.Apply(state);

            // Attacker triggers (e.g. Poison Touch)
            if (_attacker.Ability is OnHit atkAbility)
                atkAbility.Apply(state);
        }
    }

    // ── 6. End Turn ──────────────────────────────────────────────────
    public class EndTurn : IPhase
    {
        public void Run(BattleState state)
        {
            // Handle Poison/Burn/Sandstorm damage
            state.EndTurn();

            // Abilities like Speed Boost or Shed Skin trigger here
            // (You would need an OnTurnEnd decorator for this)
        }
    }
}
