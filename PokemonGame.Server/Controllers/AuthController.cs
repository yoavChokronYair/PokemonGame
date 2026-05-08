using Microsoft.AspNetCore.Mvc;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Server.Controllers
{
    // AuthController

    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("user/login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var user = _userService.GetUser(req.Username);
            if (user is null) return NotFound();
            if (user.Password.ToString() != req.HashedPassword) return Unauthorized();
            return Ok(new { Success = true, UserData = user });
        }

        [HttpPost("user/create")]
        public IActionResult CreateUser([FromBody] LoginRequest req)
        {
            if (_userService.UserExists(req.Username)) return Conflict();
            _userService.CreateUser(req.Username, req.HashedPassword.ToString());
            return Ok();
        }

        [HttpGet("user/exists/{username}")]
        public IActionResult UserExists(string username) =>
            Ok(_userService.UserExists(username));

        [HttpGet("user/{username}")]
        public IActionResult GetUser(string username)
        {
            var user = _userService.GetUser(username);
            if (user is null) return NotFound();
            return Ok(user);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
    }
}
