using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PokemonGame.Services.Factory;

namespace PokemonGame.Server.Controllers
{
    [ApiController]
    [Route("api/gamemode")]
    public class PlayerController : ControllerBase
    {
        private readonly ServiceFactory _factory;

        public PlayerController(ServiceFactory factory)
        {
            _factory = factory;
        }

        // ── GET: api/gamemode/player/{username}/{userId} ──────────────────────
        // Matches: GetOnlinePlayer(string username, int userId)
        [HttpGet("player/{username}/{userId}")]
        public IActionResult GetOnlinePlayer(string username, int userId)
        {
            var user = _factory.UserRepository.LoadUserById(userId);
            if (user is null) return NotFound("User not found.");

            var player = _factory.OnlinePlayerRepository.LoadOnlinePlayerByName(username, userId);
            if (player is null) return NotFound("Player not found.");

            return Ok(player);
        }

        // ── GET: api/gamemode/players/{userId} ────────────────────────────────
        // Matches: GetAllOnlinePlayers(int userId)
        [HttpGet("players/{userId}")]
        public IActionResult GetAllOnlinePlayers(int userId)
        {
            var user = _factory.UserRepository.LoadUserById(userId);
            if (user is null) return NotFound("User not found.");

            var players = _factory.OnlinePlayerRepository.GetAllOnlinePlayers(user);
            return Ok(players);
        }

        // ── POST: api/gamemode/player ─────────────────────────────────────────
        // Matches: CreateOnlinePlayer(string username, int userId)
        [HttpPost("player")]
        public IActionResult CreateOnlinePlayer([FromBody] CreatePlayerRequest req)
        {
            var user = _factory.UserRepository.LoadUserById(req.UserId);
            if (user is null) return NotFound("Base user not found.");

            if (_factory.OnlinePlayerRepository.OnlinePlayerExists(req.Username, user))
                return Conflict("Player name already exists for this user.");

            _factory.OnlinePlayerRepository.CreateOnlinePlayer(req.Username, user);
            return Ok();
        }

        // ── GET: api/gamemode/exists/{username}/{userId} ──────────────────────
        // Matches: PlayerExists(string username, int userId)
        [HttpGet("exists/{username}/{userId}")]
        public IActionResult PlayerExists(string username, int userId)
        {
            var user = _factory.UserRepository.LoadUserById(userId);
            if (user is null) return NotFound("User not found.");

            bool exists = _factory.OnlinePlayerRepository.OnlinePlayerExists(username, user);
            return Ok(exists);
        }

        // ── GET: api/gamemode/settings/{battlePlayerId} ───────────────────────
        // Matches: GetSettings(int battlePlayerId)
        [HttpGet("settings/{battlePlayerId}")]
        public IActionResult GetSettings(int battlePlayerId)
        {
            // Note: Update this call to match how your ServiceFactory fetches settings.
            // If your repository doesn't have a direct settings call, you can load the player 
            // first and return their settings property.
            var player = _factory.OnlinePlayerRepository.LoadOnlinePlayerByID(battlePlayerId);
            if (player is null) return NotFound("Player not found.");

            // Assuming player settings are accessible via your loaded player entity or a specialized repo:
            // return Ok(player.Settings); 

            throw new NotImplementedException("Ensure this maps to your settings retrieval logic.");
        }
    }

    // Updated to match the JSON body serialized by the Client's CreateOnlinePlayer method
    public class CreatePlayerRequest
    {
        public string Username { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
