using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Model.Domain.Move
{
    internal interface IAttampt
    {
        void Execute(BattleDomain battle);
    }
    internal class Attampt : IAttampt
    {
        //public Animation animation
        public ICondition<BattleDomain> accuracy { get; set; }
        public IEffect onHit;
        public IEffect onMiss;
        public IEffect after;

        public void Execute(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    internal class Casade : IAttampt
    {
        public List<IAttampt> attampts;

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