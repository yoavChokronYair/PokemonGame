using Microsoft.AspNetCore.Mvc;
using PokemonGame.Services.Factory;

namespace PokemonGame.Server.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class AuthController : ControllerBase
    {
        private readonly ServiceFactory _factory;

        public AuthController(ServiceFactory factory)
        {
            _factory = factory;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username)) return BadRequest();

            var result = _factory.UserService.Login(req.Username, req.HashedPassword);
            if (!result) return Unauthorized();

            var user = _factory.UserService.GetUser(req.Username);
            return Ok(new { Success = true, UserData = user });
        }

        [HttpPost("create")]
        public IActionResult CreateUser([FromBody] LoginRequest req)
        {
            var success = _factory.UserService.CreateUser(req.Username, req.HashedPassword);
            return success ? Ok() : Conflict("Username already taken.");
        }

        [HttpGet("exists/{username}")]
        public IActionResult UserExists(string username)
        {
            return Ok(_factory.UserService.UserExists(username));
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
    }
}
