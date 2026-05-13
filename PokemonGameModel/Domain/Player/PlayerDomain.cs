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

        public TrainerInfoDomain trainerInfo { get; set; } = new();
        public TrainerMapLocDomain trainerMapLocDomain { get; set; } = new();
        public PlayerTeamDomain Team { get; set; }
        public ProggressFlagDomain ProgressFlags { get; set; } = new();
        // ── Inventory ────────────────────────────────────────────────────────
        public TrainerItemDomain trainerItemDomain { get; set; } = new();

        public Dictionary<int, (bool seen, bool caught,string name)> Pokedex { get; set; } = new();

        // ── Badge count (derived) ─────────────────────────────────────────────
        public List<BadgeDomain> Badges { get; set; } = new();

        // ── Convenience methods ───────────────────────────────────────────────

        public void OnTrainerDefeated(int trainerId) =>
            ProgressFlags.DefeatedTrainers.Add(trainerId);

        public void OnItemTaken(int npcId) =>
            ProgressFlags.ItemTaken.Add(npcId);

        public void OnStoryFlagReached(int flagId) =>
            ProgressFlags.StoryFlags.Add(flagId);

        public void OnPokemonTraded(int pokedexId) =>
            ProgressFlags.TradedPokemon.Add(pokedexId);

        public bool HasStoryFlag(int flagId) =>
            ProgressFlags.StoryFlags.Contains(flagId);

        public bool HasDefeatedTrainer(int trainerId) =>
            ProgressFlags.DefeatedTrainers.Contains(trainerId);

        public bool HasTakenItem(int npcId) =>
            ProgressFlags.ItemTaken.Contains(npcId);

        public bool HasTradedPokemon(int pokedexId) =>
            ProgressFlags.TradedPokemon.Contains(pokedexId);
        public void AddBadge(int badgeId)
        {
            BadgeDomain? badge = Badges.FirstOrDefault(b => b.Id == badgeId);

            if (badge != null)
            {
                badge.IsObtained = true;
            }
        }

        public bool HasBadge(int badgeId)
        {
            return Badges.Any(b =>
                b.Id == badgeId &&
                b.IsObtained);
        }
        public int AnimationTick { get; private set; } = 0;
        public bool IsMoving { get; set; } = false;

        private static readonly int[] WalkCycle = { 0, 1, 2, 1 };

        public void AdvanceAnimation()
        {
            if (IsMoving)
                AnimationTick = (AnimationTick + 1) % WalkCycle.Length;
            else
                AnimationTick = 1; // standing frame
        }
    }
    public class BadgeDomain
    {
        public int Id { get; set; } = 0;
        public bool IsObtained { get; set; } = false;

    }
    public class TrainerInfoDomain
    {
        public int TrainerID { get; set; }
        public string Name { get; set; }
        public int Money { get; set; }
        public DateTime TimePlayed { get; set; }
        public Gender Gender { get; set; }
        public int HallOfFameDebut { get; set; }
    }
    public class ProggressFlagDomain
    {
        public HashSet<int> DefeatedTrainers { get; set; } = new();  // trainer id
        public HashSet<int> ItemTaken { get; set; } = new();          // item npc id
        public HashSet<int> StoryFlags { get; set; } = new();         // elite four, giovanni, champion progression
        public HashSet<int> TradedPokemon { get; set; } = new();      // pokedex id of pokemon given away
    }
    public class TrainerMapLocDomain
    {
        public TrainerInfoDomain trainerInfo { get; set; }
        // ── Location ─────────────────────────────────────────────────────────
        public FacingDirection FacingDirection { get; set; }
        public MapDomain LastMapVisited { get; set; }
        public MapDomain CurrentMap { get; set; }
        public (int x, int y) playerLoc { get; set; }
        public bool IsSurfing { get; set; }
    }
    public class TrainerItemDomain
    {
        public Dictionary<itemsDomain, int> BagInventory { get; set; } = new();
        public Dictionary<string, (List<PokemonPlayerDomain>, string)> BoxStorage { get; set; } = new(); // name - pokemon list - wallpaper
        public KeyItemState RegisterKey { get; set; }
        public bool HasRunningShoes { get; set; }
    }
}