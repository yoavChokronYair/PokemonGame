namespace PokemonGame.Services.Data.GameData.PokemonData
{
    public class PokemonStatsData
    {
        public int PokedexID { get; set; }

        // Maps to the 0/1 boolean in the database
        public bool IsEVYield { get; set; }

        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpAtk { get; set; }
        public int SpDef { get; set; }
        public int Speed { get; set; }

        // Helper property to calculate Base Stat Total (BST) easily in your UI
        public int BST => HP + Attack + Defense + SpAtk + SpDef + Speed;
    }
}
