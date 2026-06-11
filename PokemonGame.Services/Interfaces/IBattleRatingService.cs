namespace PokemonGame.Services.Interfaces
{
    public interface IBattleRatingService
    {
        BattleRatingResult ApplyBattleResult(
            int battlePlayerId,
            bool isSingles,
            bool playerWon);
    }

    public class BattleRatingResult
    {
        public int OldElo { get; set; }
        public int NewElo { get; set; }
        public int Delta { get; set; }
    }
}
