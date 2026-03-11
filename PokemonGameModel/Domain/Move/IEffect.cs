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
        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    //more effects
    public class Conditions : IEffect
    {
        public void onPass()
        {

        }
        public void onFail()
        {

        }
        public ICondition<BattleDomain> condition;
        public void Apply(BattleDomain battle)
        {
            throw new NotImplementedException();
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