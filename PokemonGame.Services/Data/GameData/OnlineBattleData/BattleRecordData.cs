namespace PokemonGame.Services.Data.GameData.OnlineBattleData
{
    public class BattleRecordData 
    { 
        public int BattleID { get; set; } 
        public int? WinnerBattlePlayerID { get; set; }
        public string? BattleDate { get; set; } 
    }
}
