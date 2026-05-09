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

        // ── Identity ─────────────────────────────────────────────────────────
        public int TrainerID { get; set; }
        public string Name { get; set; }
        public int Money { get; set; }
        public int TimePlayed { get; set; }
        public FacingDirection FacingDirection { get; set; }

        // ── Location ─────────────────────────────────────────────────────────
        public MapDomain LastMapVisited { get; set; }
        public MapDomain CurrentMap { get; set; }
        public (int x, int y) playerLoc { get; set; }

        // ── Team ─────────────────────────────────────────────────────────────
        public PokemonTeam Team { get; set; }

        // ── Progress flags ───────────────────────────────────────────────────
        public HashSet<int> DefeatedTrainers { get; set; } = new();  // trainer id
        public HashSet<int> ItemTaken { get; set; } = new();          // item npc id
        public HashSet<int> StoryFlags { get; set; } = new();         // elite four, giovanni, champion progression
        public HashSet<int> Badges { get; set; } = new();             // badge id per gym leader
        public HashSet<int> TradedPokemon { get; set; } = new();      // pokedex id of pokemon given away

        // ── Inventory ────────────────────────────────────────────────────────
        public Dictionary<itemsDomain, int> BagInventory { get; set; } = new();
        public Dictionary<itemsDomain, int> StorageInventory { get; set; } = new();
        public Dictionary<string, (List<PokemonState>, string)> BoxStorage { get; set; } = new(); // name - pokemon list - wallpaper

        // ── Pokédex ──────────────────────────────────────────────────────────
        public Dictionary<int, (bool seen, bool caught)> Pokedex { get; set; } = new();

        // ── Key items / abilities ─────────────────────────────────────────────
        public KeyItemState RegisterKey { get; set; }
        public bool HasRunningShoes { get; set; }
        public bool IsSurfing { get; set; }

        // ── Badge count (derived) ─────────────────────────────────────────────
        public int BadgeCount => Badges.Count;


        // ── Convenience methods ───────────────────────────────────────────────

        public void OnTrainerDefeated(int trainerId) =>
            DefeatedTrainers.Add(trainerId);

        public void OnItemTaken(int npcId) =>
            ItemTaken.Add(npcId);

        public void OnStoryFlagReached(int flagId) =>
            StoryFlags.Add(flagId);

        public void OnBadgeEarned(int badgeId) =>
            Badges.Add(badgeId);

        public void OnPokemonTraded(int pokedexId) =>
            TradedPokemon.Add(pokedexId);

        public bool HasBadge(int badgeId) =>
            Badges.Contains(badgeId);

        public bool HasStoryFlag(int flagId) =>
            StoryFlags.Contains(flagId);

        public bool HasDefeatedTrainer(int trainerId) =>
            DefeatedTrainers.Contains(trainerId);

        public bool HasTakenItem(int npcId) =>
            ItemTaken.Contains(npcId);

        public bool HasTradedPokemon(int pokedexId) =>
            TradedPokemon.Contains(pokedexId);
    }
}