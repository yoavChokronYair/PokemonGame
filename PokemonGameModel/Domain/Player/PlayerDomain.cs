using PokemonGame.Model.Domain.Item;
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
        public int Money { get; set; }
        public int TimePlayed { get; set; }
        public int BadgeCount { get; set; }
        public FacingDirection FacingDirection { get; set; }
        public PokemonTeam Team { get; set; }
        public MapDomain LastMapVisited { get; set; }
        public MapDomain CurrentMap {  get; set; }
        public (int x,int y) playerLoc { get; set; }
        public HashSet<int> DefeatedTrainers { get; set; } = new();
        public HashSet<int> ItemTaken { get; set; } = new();
        public Dictionary<itemsDomain, int> BagInventory { get; set; } = new();
        public Dictionary<itemsDomain, int> StorageInventory { get; set; } = new();
        public Dictionary<string,(List<PokemonState>, string)> BoxStorage { get; set; } = new();//name-pokemon list-wallpaer name
        public Dictionary<int,(bool,bool)> Pokedex { get; set; } = new();//pokemon id-seen, caught

    }
}
