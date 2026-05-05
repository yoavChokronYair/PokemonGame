
using Microsoft.AspNetCore.Mvc;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Factory;

namespace PokemonGame.Server.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly ServiceFactory _factory;

        public ProfileController(ServiceFactory factory)
        {
            _factory = factory;
        }

        [HttpGet("{battlePlayerId}")]
        public IActionResult GetFullProfile(int battlePlayerId)
        {
            var data = _factory.ProfileService.GetFullProfileData(battlePlayerId);
            if (data is null) return NotFound();
            return Ok(data);
        }

        [HttpPost("{battlePlayerId}/setting")]
        public IActionResult UpdateSetting(int battlePlayerId, [FromBody] UpdateSettingRequest req)
        {
            _factory.ProfileService.UpdateSetting(battlePlayerId, req.ColumnName, req.Value);
            return Ok();
        }

        [HttpPost("{battlePlayerId}/favteam")]
        public IActionResult SetFavoriteTeam(int battlePlayerId, [FromBody] SetFavoriteTeamRequest req)
        {
            _factory.ProfileService.SetFavoriteTeam(battlePlayerId, req.TeamId);
            return Ok();
        }
    }

    public class FullProfileDto
    {
        public BattlePlayerData? Player { get; set; }
        public BattlePlayerStatsData Stats { get; set; } = new();
        public BattlePlayerSettingsData Settings { get; set; } = new();
        public List<TeamData> Teams { get; set; } = new();
    }

    public class UpdateSettingRequest
    {
        public string ColumnName { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class SetFavoriteTeamRequest
    {
        public int TeamId { get; set; }
    }
}
