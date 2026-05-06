using System;
using System.Collections.Generic;
using System.Text;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Interfaces
{
    public interface IUserService
    {
        // ── Auth ──────────────────────────────────────────────────────────────
        bool Login(string username, string password);
        bool CreateUser(string username, string password);
        bool UserExists(string username);

        // ── User data ─────────────────────────────────────────────────────────
        UserData? GetUser(string username);
    }
}
