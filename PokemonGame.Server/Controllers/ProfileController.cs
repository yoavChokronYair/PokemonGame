
using Microsoft.AspNetCore.Mvc;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Server.Controllers
{
    // ProfileController

    [ApiController]
    [Route("api")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("profile/{battlePlayerId}")]
        public IActionResult GetFullProfile(int battlePlayerId)
        {
            var data = _profileService.GetFullProfileData(battlePlayerId);
            if (data.Player is null) return NotFound();
            return Ok(data);
        }

        [HttpPost("profile/{battlePlayerId}/setting")]
        public IActionResult UpdateSetting(int battlePlayerId, [FromBody] UpdateSettingRequest req)
        {
            _profileService.UpdateSetting(battlePlayerId, req.ColumnName, req.Value);
            return Ok();
        }

        [HttpPost("profile/{battlePlayerId}/favteam")]
        public IActionResult SetFavoriteTeam(int battlePlayerId, [FromBody] SetFavoriteTeamRequest req)
        {
            _profileService.SetFavoriteTeam(battlePlayerId, req.TeamId);
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
