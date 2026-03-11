using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Model.Domain.Move
{
    //probabely use a deleget
    public interface INumber
    {
        //combinators:+/6

        //product,sum,quotient 
        public double Evaluate(BattleDomain battle);
    }
    public class Product : INumber
    {
        private readonly INumber left;
        private readonly INumber right;

        public Product(INumber left, INumber right)
        {
            this.left = left;
            this.right = right;
        }

        public double Evaluate(BattleDomain battle)
        {
            return left.Evaluate(battle) * right.Evaluate(battle);
        }
    }
    public class Exactly : INumber
    {
        public double exactDamaged;

        public Exactly(double exactDamaged)
        {
            this.exactDamaged = exactDamaged;
        }

        public double Evaluate(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class Between : INumber
    {
        public double min;
        public double max;

        public double Evaluate(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class Weighted : INumber
    {
        //ToDO:understand more
        public List<double> weighted;

        public double Evaluate(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    //can be not by a battle but a battler
    public class MaxHP: INumber
    {
        public ITarget target;

        public MaxHP(ITarget target)
        {
            this.target = target;
        }

        public double Evaluate(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class CuerrentHP : INumber
    {
        public ITarget target;

        public double Evaluate(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class Level : INumber
    {
        public ITarget target;

        public double Evaluate(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    public class LastDamageDealt : INumber
    {
        public ITarget target;

        public double Evaluate(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
}