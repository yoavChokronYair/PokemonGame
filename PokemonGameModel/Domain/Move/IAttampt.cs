using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Model.Domain.Move
{
    internal interface IAttempt
    {
        void Execute(BattleDomain battle);
    }

    // Single move attempt — hits or misses, with optional after effect
    // e.g. Flamethrower: accuracy check → damage + 10% burn chance, crash on miss
    internal class Attempt : IAttempt
    {
        //public Animation animation
        public ICondition<BattleDomain> accuracy { get; set; }
        public IEffect? onHit;
        public IEffect? onMiss;
        public IEffect? after;

        public Attempt(
            ICondition<BattleDomain> accuracy,
            IEffect? onHit = null,
            IEffect? onMiss = null,
            IEffect? after = null)
        {
            this.accuracy = accuracy;
            this.onHit = onHit;
            this.onMiss = onMiss;
            this.after = after;
        }

        public void Execute(BattleDomain battle)
        {
            if (accuracy.Check(battle))
                onHit?.Apply(battle);
            else
                onMiss?.Apply(battle);

            after?.Apply(battle);
        }
    }

    // Runs multiple attempts in sequence, stops if any miss (or all, depending on move)
    // e.g. Sky Uppercut into a second hit, or a two-stage charge move
    internal class Cascade : IAttempt
    {
        private readonly List<IAttempt> attempts;
        private readonly bool stopOnMiss;

        public Cascade(List<IAttempt> attempts, bool stopOnMiss = true)
        {
            this.attempts = attempts;
            this.stopOnMiss = stopOnMiss;
        }

        // Convenience constructor for inline usage
        public Cascade(bool stopOnMiss = true, params IAttempt[] attempts)
        {
            this.attempts = new List<IAttempt>(attempts);
            this.stopOnMiss = stopOnMiss;
        }

        public void Execute(BattleDomain battle)
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

    // Hits multiple times in one turn — each hit rolls accuracy independently
    // e.g. Bullet Seed (2–5 hits), Double Kick (always 2), Fury Attack
    internal class Combo : IAttempt
    {
        //public Animation animation
        private readonly ICondition<BattleDomain> accuracy;
        private readonly INumber hits;        // how many times to attempt
        private readonly IEffect onEachHit;   // applied per successful hit
        private readonly IEffect? onEachMiss; // applied per missed hit (rare, e.g. no effect)
        private readonly IEffect? after;      // applied once after all hits resolve

        public Combo(
            ICondition<BattleDomain> accuracy,
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

        public void Execute(BattleDomain battle)
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

    // Two-turn move — charge turn then release turn
    // e.g. Solar Beam (charge → fire), Fly (vanish → strike), Skull Bash
    internal class Charge : IAttempt
    {
        private readonly IEffect chargeEffect;   // turn 1: animation, invulnerability, stat boost
        private readonly IAttempt releaseAttempt; // turn 2: the actual attack

        public Charge(IEffect chargeEffect, IAttempt releaseAttempt)
        {
            this.chargeEffect = chargeEffect;
            this.releaseAttempt = releaseAttempt;
        }

        public void Execute(BattleDomain battle)
        {
            if (!battle.ActiveUser.IsCharging)
            {
                chargeEffect.Apply(battle);
                battle.ActiveUser.BeginCharge(this);
            }
            else
            {
                battle.ActiveUser.EndCharge();
                releaseAttempt.Execute(battle);
            }
        }
    }

    // Locks the user into repeating the same move for several turns
    // e.g. Outrage (2–3 turns → confusion), Petal Dance, Thrash
    internal class Rampage : IAttempt
    {
        private readonly IAttempt attack;
        private readonly Between duration;
        private readonly IEffect afterRampage; // e.g. Confuse(user)

        public Rampage(IAttempt attack, IEffect afterRampage, int minTurns = 2, int maxTurns = 3)
        {
            this.attack = attack;
            this.afterRampage = afterRampage;
            this.duration = new Between(minTurns, maxTurns);
        }

        public void Execute(BattleDomain battle)
        {
            var user = battle.ActiveUser;

            if (!user.IsRampaging)
            {
                int turns = (int)duration.Evaluate(battle);
                user.BeginRampage(turns);
            }

            attack.Execute(battle);
            user.DecrementRampage();

            if (!user.IsRampaging)
                afterRampage.Apply(battle);
        }
    }
}