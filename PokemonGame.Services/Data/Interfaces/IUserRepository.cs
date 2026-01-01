using PokemonGame.Services.Data.GameData.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.Interfaces
{
    internal interface IUserRepository
    {
        UserData? LoadUserByName(string username);
        bool UserExists(string username);
        UserData CreateUser(string username, int passwordHash);
        List<UserData> GetAllUsers();
    }

}
