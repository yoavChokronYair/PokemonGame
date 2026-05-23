using PokemonGame.Model.Domain.Battle;

namespace PokemonGame.Model.Interface
{
    public interface IPhase
    {
        void Run(BattleState battleState);
    }
    
}
