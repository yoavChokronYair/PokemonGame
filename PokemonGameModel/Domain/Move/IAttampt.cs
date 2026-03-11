using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Model.Domain.Move
{
    internal interface IAttampt
    {
        void Execute(BattleDomain battle);
    }
    internal class Attempt : IAttampt
    {
        //public Animation animation
        public ICondition<BattleDomain> accuracy { get; set; }
        public IEffect? onHit;
        public IEffect? onMiss;
        public IEffect? after;

        public Attempt(ICondition<BattleDomain> accuracy, IEffect? onHit, IEffect? onMiss, IEffect? after)
        {
            this.accuracy = accuracy;
            this.onHit = onHit;
            this.onMiss = onMiss;
            this.after = after;
        }

        public void Execute(BattleDomain battle)
        {
            if (accuracy.Check(battle))
            {
                onHit?.Apply(battle);
            }
            else
            {
                onMiss?.Apply(battle);
            }

            after?.Apply(battle);
        }
    }
    internal class Casade : IAttampt
    {
        public List<IAttampt> attampts;

        public Casade(List<IAttampt> attampts)
        {
            this.attampts = attampts;
        }

        public void Execute(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    internal class Combo : IAttampt
    {
        //public Animation animation
        public ICondition<BattleDomain> accuracy { get; set; }
        public INumber hits;
        public IEffect everyEffect;

        public void Execute(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }

}