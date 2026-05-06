using Microsoft.AspNetCore.Mvc;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Factory;

namespace PokemonGame.Server.Controllers
{
    [ApiController]
    [Route("api/team")]
    public class TeamController : ControllerBase
    {
        private readonly ServiceFactory _factory;

        public TeamController(ServiceFactory factory)
        {
            _factory = factory;
        }

        // ── GET: api/team/{battlePlayerId} ────────────────────────────────────
        // Matches: GetTeamsByBattlePlayer(int battlePlayerId)
        [HttpGet("{battlePlayerId}")]
        public IActionResult GetTeams(int battlePlayerId) =>
            Ok(_factory.TeamService.GetTeamsByBattlePlayer(battlePlayerId));

        [HttpDelete("{teamId}")]
        public IActionResult DeleteTeam(int teamId)
        {
            _factory.TeamService.DeleteTeam(teamId);
            return Ok();
        }

        [HttpPost]
        public IActionResult SaveTeam([FromBody] SaveTeamRequest req)
        {
            var team = _factory.TeamService.SaveTeam(req.TeamName, req.BattlePlayerId, req.Slots);
            return Ok(team);
        }

        [HttpPut("{teamId}")]
        public IActionResult UpdateTeam(int teamId, [FromBody] UpdateTeamRequest req)
        {
            _factory.TeamService.UpdateTeam(teamId, req.TeamName, req.Slots);
            return Ok();
        }

        [HttpPut("{teamId}/slot")]
        public IActionResult ReplaceTeamSlot(int teamId, [FromBody] ReplaceSlotRequest req)
        {
            _factory.TeamService.ReplaceTeamSlot(teamId, req.SlotNumber, req.Pokemon);
            return Ok();
        }

        [HttpDelete("{teamId}/slot/{pokemonId}")]
        public IActionResult RemoveTeamSlot(int teamId, int pokemonId)
        {
            _factory.TeamService.RemoveTeamSlot(teamId, pokemonId);
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