using Microsoft.AspNetCore.Mvc;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Server.Controllers
{
    [ApiController]
    [Route("api")]
    public class PlayerController : ControllerBase
    {
        private readonly IGameModeChooserService _gameModeService;
        private readonly IUserService _userService;

        public PlayerController(IGameModeChooserService gameModeService, IUserService userService)
        {
            _gameModeService = gameModeService;
            _userService = userService;
        }

        [HttpGet("gamemode/player/{username}/{userId}")]
        public IActionResult GetOnlinePlayer(string username, int userId)
        {
            var user = _userService.GetUser(userId.ToString());
            if (user is null) return NotFound();
            var player = _gameModeService.GetOnlinePlayer(username, user);
            if (player is null) return NotFound();
            return Ok(player);
        }

        [HttpGet("gamemode/players/{userId}")]
        public IActionResult GetAllOnlinePlayers(int userId)
        {
            var user = _userService.GetUser(userId.ToString());
            if (user is null) return NotFound();
            return Ok(_gameModeService.GetAllOnlinePlayers(user));
        }

        [HttpPost("gamemode/player")]
        public IActionResult CreateOnlinePlayer([FromBody] CreatePlayerRequest req)
        {
            var user = _userService.GetUser(req.UserId.ToString());
            if (user is null) return NotFound();
            var created = _gameModeService.AddOnlineModePlayer(req.Username, user);
            if (!created) return Conflict();
            return Ok();
        }

        [HttpGet("gamemode/exists/{username}/{userId}")]
        public IActionResult PlayerExists(string username, int userId)
        {
            var user = _userService.GetUser(userId.ToString());
            if (user is null) return NotFound();
            return Ok(_gameModeService.UserExists(username, user));
        }
    }

    public class CreatePlayerRequest
    {
        public string Username { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
