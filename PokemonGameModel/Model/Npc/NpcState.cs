using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Npc;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Domain.Pokemon;

namespace PokemonGame.Model.Model.Npc
{ 
    public class PokemontradingNpcState : NpcDomain
    {
        public PokemonState Offered { get; set; }
        public PokemonState Requested { get; set; }

        public bool IsTradeCompleted { get; private set; }

        public override void OnDialogueFinishedTrue()
        {
            if (IsTradeCompleted)
                return;

            TradeRequested?.Invoke(this);
        }

        public void CompleteTrade()
        {
            IsTradeCompleted = true;
        }

        public event Action<PokemontradingNpcState>? TradeRequested;
    }
    public class TrainerNpcState : NpcDomain
    {
        private readonly TrainerDomain _trainerInfo;
        public PokemonTeam Team { get; set; }
        public event Action<TrainerNpcState>? BattleRequested;

        public TrainerNpcState(TrainerDomain trainerInfo)
        {
            _trainerInfo = trainerInfo;
        }
        public override void OnDialogueFinishedTrue()
        {
            if (IsDefeated())
                return;

            // TODO: connect to your battle system
            BattleRequested?.Invoke(this);
        }
        public bool IsDefeated()
        {
            return PlayerDomain.Instance.DefeatedTrainers.Contains(_trainerInfo.id);
        }

        public void MarkDefeated()
        {
            PlayerDomain.Instance.DefeatedTrainers.Add(_trainerInfo.id);
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
            
            if (!_item.IsAvailable() && PlayerDomain.Instance.ItemTaken.Contains(Id))
                return;

            ItemGiven?.Invoke(_item);
            _item.Give();
        }

        public event Action<ItemGivingDomain>? ItemGiven;
    }
    public class ShopKeeperNpcState : NpcDomain
    {
        public List<itemsDomain> ItemsForSale { get; set; } = new();

        public override void OnDialogueFinishedTrue()
        {
            ShopOpened?.Invoke(this);
        }

        public event Action<ShopKeeperNpcState>? ShopOpened;
    }
    public class PokecenterNpcState : NpcDomain
    {
        public override void OnDialogueFinishedTrue()
        {

            PlayerDomain.Instance.Team.HealAll();
            PokecenterUsed?.Invoke();
        }
        public event Action? PokecenterUsed;
    }

}
