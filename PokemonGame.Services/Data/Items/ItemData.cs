using PokemonGame.Services.Enums;
using System.Collections.Generic;

namespace PokemonGame.Services.Data.Items
{
    public class ItemData
    {
        private int itemID;
        private string itemName;
        private string itemDescription;
        private ItemPouchType itemPouch;

        public int ItemID { get => itemID; set => itemID = value; }
        public string ItemName { get => itemName; set => itemName = value; }
        public string ItemDescription { get => itemDescription; set => itemDescription = value; }
        public ItemPouchType ItemPouch { get => itemPouch; set => itemPouch = value; }
    }
}
