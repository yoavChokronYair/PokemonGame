using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Model.Domain.Move
{
    public interface IEffect
    {
        public void Apply(BattleDomain battle);
    }
    public class Sequence : IEffect
    {
        public List<IEffect> effects;

        public Sequence(List<IEffect> effects)
        {
            this.effects = effects;
        }

        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class Probability : ICondition<BattleDomain>
    {
        private readonly double probability;
        private Random random = new Random();

        public Probability(double probability)
        {
            this.probability = probability;
        }
        public bool Check(BattleDomain entity)
        {
            throw new NotImplementedException();
        }
    }
    //more effects
    public class Conditions : IEffect
    {
        private readonly ICondition<BattleDomain> condition;
        private readonly IEffect onPass;
        private readonly IEffect onFail;

        public Conditions(
            ICondition<BattleDomain> condition,
            IEffect onPass,
            IEffect onFail = null)
        {
            this.onPass = onPass;
            this.onFail = onFail;
        }

        public void Apply(BattleDomain battle)
        {
            if (condition.Check(battle))
            {
                onPass?.Apply(battle);
            }
            else
            {
                onFail?.Apply(battle);
            }
        }
    }
    public class NoEffect : IEffect
    {

        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class FormulaDamage : IEffect
    {
        public INumber number;
        public FormulaDamage(INumber effect)
        {
            this.number = effect;
        }

        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class Faint : IEffect
    {
        public ITarget target;
        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class Drain : IEffect
    {
        public ITarget target;
        public INumber drainNumber;
        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class OHKO : IEffect
    {
        public ITarget target;

        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class Paralyzed : IEffect
    {
        public ITarget target;

        public Paralyzed(ITarget target)
        {
            this.target = target;
        }

        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class RestoreHP : IEffect
    {
        public ITarget target;
        public INumber restoredHP;

        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class CrashDamaged : IEffect
    {
        public ITarget target;
        public INumber crashedDamage;

        public CrashDamaged(ITarget target, INumber crashedDamage)
        {
            this.target = target;
            this.crashedDamage = crashedDamage;
        }

        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class DirectDamaged : IEffect
    {
        public ITarget target;
        public INumber directDamage;

        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class AttackStatChanges : IEffect
    {
        public ITarget target;
        public INumber statChanged;

        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }



}