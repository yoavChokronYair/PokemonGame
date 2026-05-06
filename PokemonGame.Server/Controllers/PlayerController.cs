using Microsoft.AspNetCore.Mvc;
using PokemonGame.Services.Data.GameData.User;
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

        [HttpGet("player/{username}/{userId}")]
        public IActionResult GetOnlinePlayer(string username, int userId)
        {
            var user = new UserData { UserID = userId };
            var player = _factory.GameModeService.GetOnlinePlayer(username, user);
            if (player is null) return NotFound();
            return Ok(player);
        }

        [HttpGet("players/{userId}")]
        public IActionResult GetAllOnlinePlayers(int userId)
        {
            var user = new UserData { UserID = userId };
            var players = _factory.GameModeService.GetAllOnlinePlayers(user);
            return Ok(players);
        }

        [HttpPost("player")]
        public IActionResult CreateOnlinePlayer([FromBody] CreatePlayerRequest req)
        {
            var user = new UserData { UserID = req.UserId };
            var success = _factory.GameModeService.AddOnlineModePlayer(req.Username, user);
            return success ? Ok() : Conflict("Player already exists.");
        }

        [HttpGet("exists/{username}/{userId}")]
        public IActionResult PlayerExists(string username, int userId)
        {
            var user = new UserData { UserID = userId };
            return Ok(_factory.GameModeService.UserExists(username, user));
        }
    }

    // Updated to match the JSON body serialized by the Client's CreateOnlinePlayer method
    public class CreatePlayerRequest
    {
        public string Username { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
