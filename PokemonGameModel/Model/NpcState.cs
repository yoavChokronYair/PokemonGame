using PokemonGame.Model.Domain.NPC;
using PokemonGame.Model.Domain.Pokemon;

namespace PokemonGame.Model.Model
{ 
    public class PokemontradingNpcState : NpcDomain
    {
        public PokemonState offered { get; set; }
        public PokemonState requested { get; set; }
    }
    public class TrainerNpcState : NpcDomain
    {
        private readonly TrainerDomain _trainerInfo;
        public PokemonTeam Team { get; set; }
    }
}
