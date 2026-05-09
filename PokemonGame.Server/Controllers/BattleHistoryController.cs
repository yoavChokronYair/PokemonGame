using Microsoft.AspNetCore.Mvc;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Server.Controllers
{
    // BattleHistoryController

    [ApiController]
    [Route("api")]
    public class BattleHistoryController : ControllerBase
    {
        private readonly IBattleHistoryService _battleHistoryService;
        private readonly IUserService _userService;

        public BattleHistoryController(
            IBattleHistoryService battleHistoryService,
            IUserService userService)
        {
            _battleHistoryService = battleHistoryService;
            _userService = userService;
        }

        [HttpGet("battlehistory/{battlePlayerId}")]
        public IActionResult GetHistory(int battlePlayerId, [FromQuery] string username)
        {
            var history = _battleHistoryService.GetBattleHistoryDisplay(battlePlayerId, username);
            return Ok(history);
        }

        [HttpPost("battlehistory/battle")]
        public IActionResult CreateBattle()
        {
            var id = _battleHistoryService.SaveBattleRecord();
            return Ok(id);
        }
    }
}
