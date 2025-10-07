using System;
using System.Collections.Generic;

namespace PokemonGame.Model.Data.Player
{

    public class PlayerData
    {
        public int PlayerID { get; set; }
        public string Name { get; set; }
        public int TimePlayedMinutes { get; set; }
        public DateTime LastSave { get; set; }
        public int Money { get; set; }
        public List<int> PokemonTeamIDs { get; set; }
        public List<int> BoxPokemonIDs { get; set; }
        public List<string> Badges { get; set; }
        public List<string> StoryFlags { get; set; }
        public PokedexData Pokedex { get; set; }
        public InventoryData Inventory { get; set; }
        public LocationData Location { get; set; }
        public Dictionary<string, MapState> MapStates { get; set; }
        public TimeStateData TimeState { get; set; }
    }

    public class PokedexData
    {
        public List<int> Seen { get; set; }
        public List<int> Caught { get; set; }
    }

    public class InventoryData
    {
        public List<ItemQuantity> Items { get; set; }
    }

    public class ItemQuantity
    {
        public string Item { get; set; }
        public int Quantity { get; set; }
    }

    public class LocationData
    {
        public string Map { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class MapState
    {
        public bool CutTreeCleared { get; set; }
    }

    public class TimeStateData
    {
        public DateTime CurrentTime { get; set; }
        public string TimeOfDay { get; set; }
    }
    public class PlayerDataList
    {
        public List<PlayerData> Players { get; set; }
    }

}
