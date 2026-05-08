using Microsoft.AspNetCore.Mvc;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Server.Controllers
{
    // TeamController

    [ApiController]
    [Route("api")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet("team/{battlePlayerId}")]
        public IActionResult GetTeams(int battlePlayerId) =>
            Ok(_teamService.GetTeamsByBattlePlayer(battlePlayerId));

        [HttpDelete("team/{teamId}")]
        public IActionResult DeleteTeam(int teamId)
        {
            _teamService.DeleteTeam(teamId);
            return Ok();
        }

        [HttpPost("team")]
        public IActionResult SaveTeam([FromBody] SaveTeamRequest req)
        {
            var team = _teamService.SaveTeam(req.TeamName, req.BattlePlayerId, req.Slots);
            return Ok(team);
        }

        [HttpPut("team/{teamId}")]
        public IActionResult UpdateTeam(int teamId, [FromBody] UpdateTeamRequest req)
        {
            _teamService.UpdateTeam(teamId, req.TeamName, req.Slots);
            return Ok();
        }

        [HttpPut("team/{teamId}/slot")]
        public IActionResult ReplaceSlot(int teamId, [FromBody] ReplaceSlotRequest req)
        {
            _teamService.ReplaceTeamSlot(teamId, req.SlotNumber, req.Pokemon);
            return Ok();
        }

        [HttpDelete("team/{teamId}/slot/{pokemonId}")]
        public IActionResult RemoveSlot(int teamId, int pokemonId)
        {
            _teamService.RemoveTeamSlot(teamId, pokemonId);
            return Ok();
        }
    }

    // ── Supporting Request Models ─────────────────────────────────────────────

    public class SaveTeamRequest
    {
        public string TeamName { get; set; } = string.Empty;
        public int BattlePlayerId { get; set; }
        public List<BattlerPokemon> Slots { get; set; } = new();
    }

    public class UpdateTeamRequest
    {
        public string TeamName { get; set; } = string.Empty;
        public List<BattlerPokemon> Slots { get; set; } = new();
    }

    public class ReplaceSlotRequest
    {
        public int SlotNumber { get; set; }
        public BattlerPokemon Pokemon { get; set; } = new();
    }
}