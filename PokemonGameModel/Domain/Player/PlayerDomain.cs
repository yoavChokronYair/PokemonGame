using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Domain.Player
{
    public class PlayerDomain
    {

        private static readonly Lazy<PlayerDomain> _instance =
                    new Lazy<PlayerDomain>(() => new PlayerDomain());
        public static PlayerDomain Instance => _instance.Value;
        public int TrainerID { get; set; }
        public string Name { get; set; }
        public FacingDirection FacingDirection { get; set; }
        public PokemonTeam Team { get; set; }
        public MapDomain LastMapVisited { get; set; }
        public MapDomain CurrentMap {  get; set; }
        public (int x,int y) playerLoc { get; set; }
        public Dictionary<NpcObjectDomain, bool> HiddenItemsState { get; set; } = new();
    }
}
