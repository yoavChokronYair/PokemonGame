using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Npc;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Domain.Pokemon;

namespace PokemonGame.Model.Model.Npc
{ 
    public class PokemontradingNpcState : NpcDomain
    {
        public PokemonPlayerDomain Offered { get; set; }
        public int PokemonRequested { get; set; }

        public override void OnDialogueFinishedTrue()
        {
            if (!PlayerDomain.Instance.Team.ContainsPokemon(PokemonRequested))
                return;
            PlayerDomain.Instance.Team.TradePokemon(PlayerDomain.Instance.Team.GetPokemonAt(PlayerDomain.Instance.Team.GetPokemonIndex(PokemonRequested)), Offered);
            TradeRequested?.Invoke(this);
        }

        public event Action<PokemontradingNpcState>? TradeRequested;
    }
    // ── trainer Base ────────────────────────────────────────────────────────────────────

    public class TrainerNpcState : NpcDomain
    {
        protected readonly TrainerDomain _trainerInfo;
        public PokemonTeam Team { get; set; }

        public event Action<TrainerNpcState>? BattleRequested;
        public event Action<int>? RewardGiven;
        public event Action? BattleWon;
        public event Action? BattleLost;

        public TrainerNpcState(TrainerDomain trainerInfo)
        {
            _trainerInfo = trainerInfo;
        }

        public override void OnDialogueFinishedTrue()
        {
            if (IsDefeated()) return;
            BattleRequested?.Invoke(this);
        }

        public bool IsDefeated() =>
            PlayerDomain.Instance.ProgressFlags.DefeatedTrainers.Contains(_trainerInfo.Id);

        public virtual void OnBattleWon()
        {
            MarkDefeated();
            BattleWon?.Invoke();
            RewardGiven?.Invoke(CalculateReward(Team));
        }

        public void OnBattleLost()
        {
            PlayerDomain.Instance.trainerInfo.Money = Math.Max(0, PlayerDomain.Instance.trainerInfo.Money - CalculateReward(Team) / 2);
            PlayerDomain.Instance.Team.HealAll();
            PlayerDomain.Instance.trainerMapLocDomain.CurrentMap = PlayerDomain.Instance.trainerMapLocDomain.LastMapVisited;
            PlayerDomain.Instance.trainerMapLocDomain.playerLoc = PlayerDomain.Instance.trainerMapLocDomain.CurrentMap.FlyWrapLoc;
            BattleLost?.Invoke();
        }

        protected void MarkDefeated() =>
            PlayerDomain.Instance.ProgressFlags.DefeatedTrainers.Add(_trainerInfo.Id);

        public int CalculateReward(PokemonTeam team)
        {
            int maxLevel = team
                .GetSwitchableIndices()
                .Select(i => team.GetPokemonAt(i).Level)
                .DefaultIfEmpty(1)
                .Max();

            return _trainerInfo.BaseMoney * maxLevel;
        }
    }

    // ── Item reward trainer ──────────────────────────────────────────────────────

    public class ItemRewardTrainerNpcState : TrainerNpcState
    {
        private readonly ItemGivingDomain _rewardItem;

        public event Action<ItemGivingDomain>? ItemRewarded;

        public ItemRewardTrainerNpcState(TrainerDomain trainerInfo, ItemGivingDomain rewardItem)
            : base(trainerInfo)
        {
            _rewardItem = rewardItem;
        }

        public override void OnBattleWon()
        {
            base.OnBattleWon();
            if (_rewardItem.IsAvailable())
            {
                _rewardItem.Give();
                ItemRewarded?.Invoke(_rewardItem);
            }
        }
    }

    // ── Gym Leader ───────────────────────────────────────────────────────────────

    public class GymLeaderNpcState : TrainerNpcState
    {
        private readonly int _badgeId;
        private readonly ItemsDomain _tm;

        public event Action<int, ItemsDomain>? GymCleared; // badgeId + TM

        public GymLeaderNpcState(TrainerDomain trainerInfo, int badgeId, ItemsDomain tm)
            : base(trainerInfo)
        {
            _badgeId = badgeId;
            _tm = tm;
        }

        public override void OnBattleWon()
        {
            base.OnBattleWon();
            PlayerDomain.Instance.AddBadge(_badgeId);
            GymCleared?.Invoke(_badgeId, _tm);
        }
    }

    // ── Gauntlet base (Elite Four + Champion) ────────────────────────────────────

    public abstract class GauntletTrainerNpcState : TrainerNpcState
    {
        protected readonly int _progressionFlagId;

        public event Action<int>? GauntletProgressionReached; // flagId

        protected GauntletTrainerNpcState(TrainerDomain trainerInfo, int progressionFlagId)
            : base(trainerInfo)
        {
            _progressionFlagId = progressionFlagId;
        }

        public override void OnBattleWon()
        {
            base.OnBattleWon();
            PlayerDomain.Instance.ProgressFlags.StoryFlags.Add(_progressionFlagId);
            GauntletProgressionReached?.Invoke(_progressionFlagId);
        }
    }

    // ── Elite Four ───────────────────────────────────────────────────────────────

    public class EliteFourNpcState : GauntletTrainerNpcState
    {
        public EliteFourNpcState(TrainerDomain trainerInfo, int progressionFlagId)
            : base(trainerInfo, progressionFlagId) { }
    }

    // ── Champion ─────────────────────────────────────────────────────────────────

    public class ChampionNpcState : GauntletTrainerNpcState
    {
        public event Action? CreditsTriggered;

        public ChampionNpcState(TrainerDomain trainerInfo, int progressionFlagId)
            : base(trainerInfo, progressionFlagId) { }

        public override void OnBattleWon()
        {
            base.OnBattleWon();
            CreditsTriggered?.Invoke();
        }
    }

    // ── Giovanni ─────────────────────────────────────────────────────────────────

    public class GiovanniNpcState : TrainerNpcState
    {
        private readonly int _storyFlagId;

        public event Action<int>? RocketBossDefeated; // storyFlagId

        public GiovanniNpcState(TrainerDomain trainerInfo, int storyFlagId)
            : base(trainerInfo)
        {
            _storyFlagId = storyFlagId;
        }

        public override void OnBattleWon()
        {
            base.OnBattleWon();
            PlayerDomain.Instance.ProgressFlags.StoryFlags.Add(_storyFlagId);
            RocketBossDefeated?.Invoke(_storyFlagId);
        }
    }

    public class ItemGiverNpcState : NpcDomain
    {
        private readonly ItemGivingDomain _item;

        public ItemGiverNpcState(ItemGivingDomain item)
        {
            _item = item;
        }

        public override void OnDialogueFinishedTrue()
        {
            
            if (!_item.IsAvailable() && PlayerDomain.Instance.ProgressFlags.ItemTaken.Contains(Id))
                return;

            ItemGiven?.Invoke(_item);
            _item.Give();
        }

        public event Action<ItemGivingDomain>? ItemGiven;
    }

    public enum ShopMode { Buy, Sell }
    public class ShopKeeperNpcState : NpcDomain
    {
        public List<ItemsDomain> ItemsForSale { get; set; } = new();

        public override void OnDialogueFinishedTrue()  // buy node landed
        {
            ShopOpened?.Invoke(this, ShopMode.Buy);
        }

        public override void OnDialogueFinishedFalse() // sell node landed
        {
            ShopOpened?.Invoke(this, ShopMode.Sell);
        }

        public event Action<ShopKeeperNpcState, ShopMode>? ShopOpened;
    }

    public class PokecenterNpcState : NpcDomain
    {
        public override void OnDialogueFinishedTrue()
        {

            PlayerDomain.Instance.Team.HealAll();
            PlayerDomain.Instance.trainerMapLocDomain.LastMapVisited = PlayerDomain.Instance.trainerMapLocDomain.CurrentMap;
            PokecenterUsed?.Invoke();
        }
        public event Action? PokecenterUsed;
    }
}
