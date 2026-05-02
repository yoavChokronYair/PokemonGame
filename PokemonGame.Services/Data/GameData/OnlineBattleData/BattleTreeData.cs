namespace PokemonGame.Services.Data.GameData.OnlineBattleData
{
    public class BattleTreeData
    {
        public int BattleID { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string OpponentName { get; set; } = string.Empty;
        public bool IsPlayerWinner { get; set; }
        public string BattleDate { get; set; } = string.Empty;
        public List<BattleHistoryPokemon> PlayerPokemon { get; set; } = new();
        public List<BattleHistoryPokemon> OpponentPokemon { get; set; } = new();
    }
    public class BattleHistoryPokemon
    {
        public int PokedexId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ItemName { get; set; } = "None";
        public string? Type1 { get; set; }
        public string? Type2 { get; set; }
    }
}
