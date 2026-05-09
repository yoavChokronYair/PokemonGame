

namespace PokemonGame.Services.Interfaces
{
    public interface IMatchmakingService
    {
        Task ConnectAsync();
        Task FindMatchAsync(MatchmakingRequest request);
        Task CancelAsync(int playerId);
        void Disconnect();

        event Action<MatchFoundData>? OnMatchFound;
        event Action? OnQueued;
        event Action? OnCancelled;

    }

    public class MatchmakingRequest
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string BattleMode { get; set; } = string.Empty;
        public bool IsOneVOne { get; set; }
        public int TeamId { get; set; }
        public List<int> SelectedPokemonIds { get; set; } = new();
    }

    public class MatchFoundData
    {
        public string SessionId { get; set; } = string.Empty;
        public int OpponentId { get; set; }
        public string OpponentName { get; set; } = string.Empty;
        public string BattleMode { get; set; } = string.Empty;
        public bool IsOneVOne { get; set; }
    }
}
