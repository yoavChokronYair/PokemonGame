using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Domain.Pokemon
{
    public class EvolutionDomain
    {
        public int PokemonId { get; set; }
        public int ToPokemonId { get; set; }
        public EvoTriggerType TriggerType { get; set; }
        public ICondition<PokemonState> Condition { get; set; }

    }
}
