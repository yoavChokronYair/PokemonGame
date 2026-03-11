namespace PokemonGame.Services.Data.GameData.User
{
    public class BattlePlayerData : UserData
    {
        public int BattlePlayerID { get; set; }   // corresponds to BattlePlayerID in DB
        public int BattleID { get; set; }         // the Battle this player is part of
        public string Name { get; set; }          // Battle-specific name
        public int Level { get; set; }
    }
}
