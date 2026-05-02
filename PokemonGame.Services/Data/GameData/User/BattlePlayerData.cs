namespace PokemonGame.Services.Data.GameData.User
{
    public class BattlePlayerData
    {
        public int BattlePlayerID { get; set; }
        public int UserID { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public string? CreatedAt { get; set; }
    }
}
