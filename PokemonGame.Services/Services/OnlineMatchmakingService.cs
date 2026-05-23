using Microsoft.AspNetCore.SignalR.Client;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Services
{
    public class OnlineMatchmakingService : IMatchmakingService, IAsyncDisposable
    {
        private readonly HubConnection _connection;
        private readonly List<IDisposable> _handlerSubscriptions = new();
        private bool _disposed;

        public event Action<MatchFoundData>? OnMatchFound;
        public event Action? OnQueued;
        public event Action? OnCancelled;
        public event Action<Exception>? OnError;

        public OnlineMatchmakingService(string serverBaseUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl($"{serverBaseUrl}/hubs/matchmaking")
                .WithAutomaticReconnect()
                .Build();

            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            _handlerSubscriptions.Add(
                _connection.On<MatchFoundData>("MatchFound", data =>
                {
                    OnMatchFound?.Invoke(data);
                }));

            _handlerSubscriptions.Add(
                _connection.On("Queued", () =>
                {
                    OnQueued?.Invoke();
                }));

            _handlerSubscriptions.Add(
                _connection.On("SearchCancelled", () =>
                {
                    OnCancelled?.Invoke();
                }));

            _connection.Reconnecting += error =>
            {
                if (error != null)
                    NotifyError(error);

                return Task.CompletedTask;
            };

            _connection.Closed += error =>
            {
                if (error != null)
                    NotifyError(error);

                return Task.CompletedTask;
            };
        }

        public async Task ConnectAsync()
        {
            ThrowIfDisposed();

            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                throw;
            }
        }

        public async Task FindMatchAsync(MatchmakingRequest request)
        {
            ThrowIfDisposed();

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            await ConnectAsync();

            try
            {
                await _connection.InvokeAsync("FindMatch", new MatchmakingRequestDto
                {
                    PlayerId = request.PlayerId,
                    PlayerName = request.PlayerName,
                    BattleMode = request.BattleMode,
                    IsOneVOne = request.IsOneVOne,
                    TeamId = request.TeamId,
                    SelectedPokemonIds = request.SelectedPokemonIds?.ToList() ?? new List<int>()
                });
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                throw;
            }
        }

        public async Task CancelAsync(int playerId)
        {
            ThrowIfDisposed();

            await ConnectAsync();

            try
            {
                await _connection.InvokeAsync("CancelSearch", playerId);
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (var subscription in _handlerSubscriptions)
            {
                subscription.Dispose();
            }

            _handlerSubscriptions.Clear();

            try
            {
                await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
        }

        private void NotifyError(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[OnlineMatchmakingService] {ex.Message}");

            OnError?.Invoke(ex);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OnlineMatchmakingService));
        }
    }

    public class MatchmakingRequestDto
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string BattleMode { get; set; } = string.Empty;
        public bool IsOneVOne { get; set; }
        public int? TeamId { get; set; }
        public List<int> SelectedPokemonIds { get; set; } = new();
    }
}