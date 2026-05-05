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
            if (string.IsNullOrWhiteSpace(req.Username) ||
                string.IsNullOrWhiteSpace(req.HashedPassword))
                return BadRequest("Missing credentials.");

            var user = _factory.UserRepository.LoadUserByName(req.Username);
            if (user is null) return NotFound("User not found.");
            if (user.Password != req.HashedPassword) return Unauthorized("Invalid password.");

            return Ok(user);
        }

        [HttpPost("create")]
        public IActionResult CreateUser([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) ||
                string.IsNullOrWhiteSpace(req.HashedPassword))
                return BadRequest("Missing credentials.");

            if (_factory.UserRepository.UserExists(req.Username))
                return Conflict("Username already taken.");

            _factory.UserRepository.CreateUser(req.Username, req.HashedPassword);
            return Ok();
        }

        [HttpGet("exists/{username}")]
        public IActionResult UserExists(string username)
        {
            return Ok(_factory.UserRepository.UserExists(username));
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
    }
}
