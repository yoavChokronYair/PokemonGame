// Design: Strategy pattern — each class is a concrete execution strategy for one attempt.
// Attempt: single hit with accuracy check.
// Cascade: sequential hits, stops on miss.
// Combo: multi-hit with random count (Bullet Seed, Fury Attack).
// Charge: two-turn charge-then-release (Solar Beam, Fly).
// Rampage: multi-turn lock with after-effect (Outrage, Thrash).
// Layer: Domain/Move — move attempt implementations.
// IAttempt interface lives in Interface/Move/IAttempt.cs.

using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Model.Helper.DesignPatterns
{
    // Single move attempt — hits or misses, with optional after effect.
    // e.g. Flamethrower: accuracy check → damage + 10% burn chance, crash on miss.
    internal class Attempt : IAttempt
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
                onHit?.Apply(battle);
            else
                onMiss?.Apply(battle);

            after?.Apply(battle);
        }
    }

    // Runs multiple attempts in sequence, stops if any miss (or all, depending on move).
    // e.g. Triple Kick — each kick has independent accuracy.
    internal class Cascade : IAttempt
    {
        private readonly List<IAttempt> attempts;
        private readonly bool stopOnMiss;

        public Cascade(List<IAttempt> attempts, bool stopOnMiss = true)
        {
            this.attempts = attempts;
            this.stopOnMiss = stopOnMiss;
        }

        public Cascade(bool stopOnMiss = true, params IAttempt[] attempts)
        {
            this.attempts = new List<IAttempt>(attempts);
            this.stopOnMiss = stopOnMiss;
        }

        public void Execute(BattleState battle)
        {
            foreach (var attempt in attempts)
            {
                bool hitLanded = attempt is Attempt a && a.accuracy.Check(battle);
                attempt.Execute(battle);
                if (stopOnMiss && !hitLanded)
                    return;
            }
        }
    }

    // Hits multiple times in one turn — each hit rolls accuracy independently.
    // e.g. Bullet Seed (2-5 hits), Double Kick (always 2), Fury Attack.
    internal class Combo : IAttempt
    {
        private readonly ICondition<BattleState> accuracy;
        private readonly INumber hits;
        private readonly IEffect onEachHit;
        private readonly IEffect? onEachMiss;
        private readonly IEffect? after;

        public Combo(
            ICondition<BattleState> accuracy,
            INumber hits,
            IEffect onEachHit,
            IEffect? onEachMiss = null,
            IEffect? after = null)
        {
            this.accuracy = accuracy;
            this.hits = hits;
            this.onEachHit = onEachHit;
            this.onEachMiss = onEachMiss;
            this.after = after;
        }

        public void Execute(BattleState battle)
        {
            int hitCount = (int)hits.Evaluate(battle);

            for (int i = 0; i < hitCount; i++)
            {
                if (accuracy.Check(battle))
                    onEachHit.Apply(battle);
                else
                    onEachMiss?.Apply(battle);
            }

            after?.Apply(battle);
        }
    }

    // Two-turn move — charge turn then release turn.
    // e.g. Solar Beam (charge → fire), Fly (vanish → strike), Skull Bash.
    internal class Charge : IAttempt
    {
        private readonly IEffect chargeEffect;
        private readonly IAttempt releaseAttempt;

        public Charge(IEffect chargeEffect, IAttempt releaseAttempt)
        {
            this.chargeEffect = chargeEffect;
            this.releaseAttempt = releaseAttempt;
        }

        public void Execute(BattleState battle)
        {
            if (!battle.Attacker.IsCharging())
            {
                chargeEffect.Apply(battle);
                battle.Attacker.BeginCharge(this);
            }
            else
            {
                battle.Attacker.EndCharge();
                releaseAttempt.Execute(battle);
            }
        }
    }

    // Locks the user into repeating the same move for several turns.
    // e.g. Outrage (2-3 turns → confusion), Petal Dance, Thrash.
    internal class Rampage : IAttempt
    {
        private readonly IAttempt attack;
        private readonly Between duration;
        private readonly IEffect afterRampage;

        public Rampage(IAttempt attack, IEffect afterRampage, int minTurns = 2, int maxTurns = 3)
        {
            this.attack = attack;
            this.afterRampage = afterRampage;
            this.duration = new Between(minTurns, maxTurns);
        }

        public void Execute(BattleState battle)
        {
            var user = battle.Attacker;

            if (!user.IsRampaging())
            {
                int turns = (int)duration.Evaluate(battle);
                user.BeginRampage(turns);
            }

            attack.Execute(battle);
            user.DecrementRampage();

            if (!user.IsRampaging())
                afterRampage.Apply(battle);
        }
    }
}
