// Layer: Interface — generic condition and target contracts used across the move system.
// ICondition<T>: checks a boolean condition against an entity of type T.
// ITarget: resolves to a PokemonDomain given the current battle state.

using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Interface.Move
{
    // Generic condition — can check against BattleDomain or PokemonDomain
    internal interface ICondition<T>
    {
        bool Check(T entity);
    }

    // Resolves which Pokemon is the target for an effect or condition
    internal interface ITarget
    {
        PokemonDomain Resolve(BattleState battle);
    }
}
