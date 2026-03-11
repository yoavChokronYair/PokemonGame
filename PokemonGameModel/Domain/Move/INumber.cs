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
    public class Exactly : INumber
    {
        public double exactDamaged;

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