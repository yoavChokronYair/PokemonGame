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
        public IActionResult GetTeams(int battlePlayerId)
        {
            var teams = _factory.TeamRepository.GetTeamsByBattlePlayer(battlePlayerId);
            return Ok(teams);
        }

        // ── DELETE: api/team/{teamId} ──────────────────────────────────────────
        // Matches: DeleteTeam(int teamId)
        [HttpDelete("{teamId}")]
        public IActionResult DeleteTeam(int teamId)
        {
            _factory.TeamRepository.DeleteTeam(teamId);
            return Ok();
        }

        // ── POST: api/team ────────────────────────────────────────────────────
        // Matches: SaveTeam(string teamName, int battlePlayerId, List<BattlerPokemon> slots)
        [HttpPost]
        public IActionResult SaveTeam([FromBody] SaveTeamRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.TeamName))
                return BadRequest("Team name required.");

            // Create the new team
            var team = _factory.TeamRepository.CreateTeam(req.TeamName, req.BattlePlayerId);

            // Populate slots (Max 6)
            for (int i = 0; i < req.Slots.Count && i < 6; i++)
            {
                var pokemonId = _factory.BattlerPokemonRepository.CreatePokemonInstance(req.Slots[i]);
                if (pokemonId > 0)
                {
                    _factory.TeamMemberRepository.SetPokemonInSlot(team.Id, pokemonId, i + 1);
                }
            }

            return Ok(team);
        }

        // ── PUT: api/team/{teamId} ────────────────────────────────────────────
        // Matches: UpdateTeam(int teamId, string teamName, List<BattlerPokemon> slots)
        [HttpPut("{teamId}")]
        public IActionResult UpdateTeam(int teamId, [FromBody] UpdateTeamRequest req)
        {
            // Update the team name
            _factory.TeamRepository.UpdateTeamName(teamId, req.TeamName);

            // Remove existing slots/instances
            foreach (var member in _factory.TeamMemberRepository.GetTeamMembers(teamId))
            {
                _factory.BattlerPokemonRepository.DeletePokemonInstance(member.PokemonID);
                _factory.TeamMemberRepository.RemovePokemonFromTeam(teamId, member.PokemonID);
            }

            // Save new slots (Max 6)
            for (int i = 0; i < req.Slots.Count && i < 6; i++)
            {
                var pokemonId = _factory.BattlerPokemonRepository.CreatePokemonInstance(req.Slots[i]);
                if (pokemonId > 0)
                {
                    _factory.TeamMemberRepository.SetPokemonInSlot(teamId, pokemonId, i + 1);
                }
            }

            return Ok();
        }

        // ── PUT: api/team/{teamId}/slot ────────────────────────────────────────
        // Matches: ReplaceTeamSlot(int teamId, int slotNumber, BattlerPokemon pokemon)
        [HttpPut("{teamId}/slot")]
        public IActionResult ReplaceTeamSlot(int teamId, [FromBody] ReplaceSlotRequest req)
        {
            // 1. Check if a slot already exists here and remove its database instance
            var existingMembers = _factory.TeamMemberRepository.GetTeamMembers(teamId);
            var targetMember = existingMembers.FirstOrDefault(m => m.SlotNumber == req.SlotNumber);
            if (targetMember != null)
            {
                _factory.BattlerPokemonRepository.DeletePokemonInstance(targetMember.PokemonID);
                _factory.TeamMemberRepository.RemovePokemonFromTeam(teamId, targetMember.PokemonID);
            }

            // 2. Create the new Pokemon instance
            var newPokemonId = _factory.BattlerPokemonRepository.CreatePokemonInstance(req.Pokemon);
            if (newPokemonId <= 0)
            {
                return BadRequest("Failed to instantiate replacement Pokemon.");
            }

            // 3. Bind new instance to the requested slot number
            _factory.TeamMemberRepository.SetPokemonInSlot(teamId, newPokemonId, req.SlotNumber);
            return Ok();
        }

        // ── DELETE: api/team/{teamId}/slot/{pokemonId} ─────────────────────────
        // Matches: RemoveTeamSlot(int teamId, int pokemonId)
        [HttpDelete("{teamId}/slot/{pokemonId}")]
        public IActionResult RemoveTeamSlot(int teamId, int pokemonId)
        {
            _factory.BattlerPokemonRepository.DeletePokemonInstance(pokemonId);
            _factory.TeamMemberRepository.RemovePokemonFromTeam(teamId, pokemonId);
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