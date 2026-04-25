using PokemonGame.Model.Domain.Npc;
using PokemonGame.Model.Domain.Pokemon;

namespace PokemonGame.Model.Model.Npc
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
