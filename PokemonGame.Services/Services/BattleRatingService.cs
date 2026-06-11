using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Services
{
    public class BattleRatingService : IBattleRatingService
    {
        private readonly BattlePlayerStatsRepository _statsRepo;

        public BattleRatingService()
        {
            _statsRepo = ServiceFactory.Instance.BattlePlayerStatsRepository;
        }

        public BattleRatingResult ApplyBattleResult(
            int battlePlayerId,
            bool isSingles,
            bool playerWon)
        {
            var stats = _statsRepo.GetStats(battlePlayerId);

            int oldElo = isSingles
                ? stats.CurrentElo1v1
                : stats.CurrentElo2v2;

            int delta = playerWon ? 22 : -18;
            int newElo = Math.Max(0, oldElo + delta);

            _statsRepo.UpdateElo(battlePlayerId, newElo, isSingles);

            if (playerWon)
                _statsRepo.RegisterWin(battlePlayerId, isSingles);
            else
                _statsRepo.RegisterLoss(battlePlayerId, isSingles);

            return new BattleRatingResult
            {
                OldElo = oldElo,
                NewElo = newElo,
                Delta = newElo - oldElo
            };
        }
    }
}