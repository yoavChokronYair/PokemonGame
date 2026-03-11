namespace PokemonGame.Services.Data.GameData.User.OnlinePlayer
{
    public class OnlineFriend
    {
        public int UserID { get; set; }
        public int FriendUserID { get; set; }

        public string Username { get; set; }
        public bool IsOnline { get; set; }
    }

}
