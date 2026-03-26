namespace PokemonGame.Services.Data.GameData.User
{
    public class BattlePlayerData
    {
        public int BattlePlayerID { get; set; }
        public int? BattleID { get; set; }
        public int UserID { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public string? CreatedAt { get; set; }
    }
    public class BattlePlayerFriendData
    {
        public int PlayerID { get; set; }
        public int FriendPlayerID { get; set; }
        public string? Status { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Level { get; set; }
        public string? LastPlayed { get; set; }
    }
}
