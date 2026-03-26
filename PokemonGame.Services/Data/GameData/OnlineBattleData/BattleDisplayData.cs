namespace PokemonGame.Services.Data.GameData.OnlineBattleData
{
    public class BattleDisplayData
    {
        public int BattleID { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string OpponentName { get; set; } = string.Empty;
        public bool IsPlayerWinner { get; set; }
        public List<string> PlayerPokemon { get; set; } = new();
        public List<string> OpponentPokemon { get; set; } = new();
    }
}
