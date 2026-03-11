using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;

namespace PokemonGame.Model.Domain.Move
{
    //ToDo:Use deleget
    public interface ICondition<T>
    {
        //and(t),or(t),not(t),probality(t)
        public bool Check(T entity);
    }
    public interface ITarget
    {
        //for effects as well
        //attacker or defender
        public PokemonData Resolve(BattleDomain battle);
    }
    //internal interface ICondition<Inumber>
    //{
    //    //conditions:
    //    //less
    //    //greater
    //}
    //internal interface ICondition<BattleDomain>
    //{
    //    //conditions:
    //    //For who: Itarget
    //    //
    //}
    //internal interface ICondition<pokemonData>
    //{
    //    //conditions:
    //    //HasElement
    //    //IsParlayzed
    //}

    

}