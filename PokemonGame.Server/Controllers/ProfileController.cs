
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

        // ── Full profile fetch (stats + settings + teams) ─────────────────────
        [HttpGet("{battlePlayerId}")]
        public IActionResult GetFullProfile(int battlePlayerId)
        {
            var player = _factory.OnlinePlayerRepository.LoadOnlinePlayerByID(battlePlayerId);
            var stats = _factory.BattlePlayerStatsRepository.GetStats(battlePlayerId);
            var settings = _factory.BattlePlayerSettingsRepository.GetSettings(battlePlayerId);
            var teams = _factory.TeamRepository.GetTeamsByBattlePlayer(battlePlayerId);

            if (player is null) return NotFound();

            return Ok(new FullProfileDto
            {
                Player = player,
                Stats = stats,
                Settings = settings,
                Teams = teams
            });
        }

        // ── Update a single setting ───────────────────────────────────────────
        [HttpPost("{battlePlayerId}/setting")]
        public IActionResult UpdateSetting(int battlePlayerId, [FromBody] UpdateSettingRequest req)
        {
            _factory.BattlePlayerSettingsRepository.SaveSetting(
                battlePlayerId, req.ColumnName, req.Value);
            return Ok();
        }

        // ── Set favourite team ────────────────────────────────────────────────
        [HttpPost("{battlePlayerId}/favteam")]
        public IActionResult SetFavoriteTeam(int battlePlayerId, [FromBody] SetFavoriteTeamRequest req)
        {
            _factory.BattlePlayerStatsRepository.SaveFaveTeam(battlePlayerId, req.TeamId);
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
