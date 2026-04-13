// Design: Strategy pattern — each class is a concrete execution strategy for one attempt.
// Attempt: single hit with accuracy check.
// Cascade: sequential hits, stops on miss.
// Combo: multi-hit with random count (Bullet Seed, Fury Attack).
// Charge: two-turn charge-then-release (Solar Beam, Fly).
// Rampage: multi-turn lock with after-effect (Outrage, Thrash).
// Layer: Domain/Move — move attempt implementations.
// IAttempt interface lives in Interface/Move/IAttempt.cs.

using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.DesignPatterns
{
    // Single move attempt — hits or misses, with optional after effect.
    // e.g. Flamethrower: accuracy check → damage + 10% burn chance, crash on miss.
    public class Attempt : IAttempt
    {
        public ICondition<BattleState> accuracy { get; set; }
        public IEffect? onHit;
        public IEffect? onMiss;
        public IEffect? after;

        public Attempt(
            ICondition<BattleState> accuracy,
            IEffect? onHit = null,
            IEffect? onMiss = null,
            IEffect? after = null)
        {
            this.accuracy = accuracy;
            this.onHit = onHit;
            this.onMiss = onMiss;
            this.after = after;
        }

        public void Execute(BattleState battle)
        {
            if (accuracy.Check(battle))
            {
                onHit?.Apply(battle);
            }
            else
            {
                onMiss?.Apply(battle);
                battle.Logger.Log($"but it failed");
            }

            after?.Apply(battle);
        }
    }

    // Runs multiple attempts in sequence, stops if any miss (or all, depending on move).
    // e.g. Triple Kick — each kick has independent accuracy.
    public class Cascade : IAttempt
    {
        private readonly List<IAttempt> _attempts;
        private readonly bool _stopOnMiss;

        public Cascade(List<IAttempt> attempts, bool stopOnMiss = true)
        {
            _attempts = attempts;
            _stopOnMiss = stopOnMiss;
        }

        public Cascade(bool stopOnMiss = true, params IAttempt[] attempts)
        {
            _attempts = new List<IAttempt>(attempts);
            _stopOnMiss = stopOnMiss;
        }

        public void Execute(BattleState battle)
        {
            foreach (var attempt in _attempts)
            {
                bool hitLanded = attempt is Attempt a && a.accuracy.Check(battle);
                attempt.Execute(battle);
                if (_stopOnMiss && !hitLanded)
                {
                    return;
                }
            }
        }
    }

    // Hits multiple times in one turn — each hit rolls accuracy independently.
    // e.g. Bullet Seed (2-5 hits), Double Kick (always 2), Fury Attack.
    public class Combo : IAttempt
    {
        private readonly ICondition<BattleState> _accuracy;
        private readonly INumber _hits;
        private readonly IEffect _onEachHit;
        private readonly IEffect? _onEachMiss;
        private readonly IEffect? _after;

        public Combo(
            ICondition<BattleState> accuracy,
            INumber hits,
            IEffect onEachHit,
            IEffect? onEachMiss = null,
            IEffect? after = null)
        {
            _accuracy = accuracy;
            _hits = hits;
            _onEachHit = onEachHit;
            _onEachMiss = onEachMiss;
            _after = after;
        }

        public void Execute(BattleState battle)
        {
            int hitCount = (int)_hits.Evaluate(battle);

            for (int i = 0; i < hitCount; i++)
            {
                if (_accuracy.Check(battle))
                {
                    _onEachHit.Apply(battle);
                }
                else
                {
                    _onEachMiss?.Apply(battle);
                }
            }

            _after?.Apply(battle);
        }
    }

    // Two-turn move — charge turn then release turn.
    // e.g. Solar Beam (charge → fire), Fly (vanish → strike), Skull Bash.
    public class Charge : IAttempt
    {
        private readonly IEffect _chargeEffect;
        private readonly IAttempt _releaseAttempt;

        public Charge(IEffect chargeEffect, IAttempt releaseAttempt)
        {
            _chargeEffect = chargeEffect;
            _releaseAttempt = releaseAttempt;
        }

        public void Execute(BattleState battle)
        {
            if (!battle.Attacker.IsCharging())
            {
                _chargeEffect.Apply(battle);
                battle.Attacker.BeginCharge(this);
            }
            else
            {
                battle.Attacker.EndCharge();
                _releaseAttempt.Execute(battle);
            }
        }
    }

    // Locks the user into repeating the same move for several turns.
    // e.g. Outrage (2-3 turns → confusion), Petal Dance, Thrash.
    public class Rampage : IAttempt
    {
        private readonly IAttempt _attack;
        private readonly Between _duration;
        private readonly IEffect _afterRampage;

        public Rampage(IAttempt attack, IEffect afterRampage, int minTurns = 2, int maxTurns = 3)
        {
            _attack = attack;
            _afterRampage = afterRampage;
            _duration = new Between(minTurns, maxTurns);
        }

        public void Execute(BattleState battle)
        {
            var user = battle.Attacker;

            if (!user.IsRampaging())
            {
                int turns = (int)_duration.Evaluate(battle);
                user.BeginRampage(turns);
            }

            _attack.Execute(battle);
            user.DecrementRampage();

            if (!user.IsRampaging())
            {
                _afterRampage.Apply(battle);
            }
        }
    }
}
