using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Interfaces
{
    public interface IGameModeChooserService
    {
        bool AddOnlineModePlayer(string username, UserData user);
        bool OnlinePlayerLogIn(string username, UserData user);
        bool UserExists(string username, UserData user);
        BattlePlayerData? GetOnlinePlayer(string username, UserData user);
        List<BattlePlayerData> GetAllOnlinePlayers(UserData user);
    }
}
