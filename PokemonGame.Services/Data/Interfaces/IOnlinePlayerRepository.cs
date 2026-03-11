using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Data.Interfaces
{
    internal interface IOnlinePlayerRepository
    {
        BattlePlayerData CreateOnlinePlayer(string username, UserData user);
        bool OnlinePlayerExists(string username, UserData user);
        BattlePlayerData? LoadOnlinePlayerByName(string username, UserData user);
        List<BattlePlayerData> GetAllOnlinePlayers(UserData user);
        BattlePlayerData? LoadOpponentPlayer(BattlePlayerData player, int battleID);
    }

}
