using Microsoft.AspNetCore.SignalR.Client;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Services
{
    public static class OnlineBattleActionTypes
    {
        public const string Move = "Move";
        public const string Switch = "Switch";
        public const string Forfeit = "Forfeit";

        public static bool IsValid(string actionType)
        {
            return actionType == Move ||
                   actionType == Switch ||
                   actionType == Forfeit;
        }
    }

    public class OnlineBattleService : IBattleService
    {
        private readonly HubConnection _connection;
        private readonly string _sessionId;
        private readonly int _playerId;
        public event Action<OnlineConnectionStatus>? OnConnectionStatusChanged;

        private BattleSnapshot? _lastSnapshot;
        private Task? _connectTask;

        public event Action? OnStateUpdated;
        public event Action<Exception>? OnError;

        public bool IsConnected => _connection.State == HubConnectionState.Connected;
        public bool HasInitialState => _lastSnapshot != null;
        public bool IsOver => _lastSnapshot?.IsOver ?? false;
        public string? WinnerName => _lastSnapshot?.WinnerName;
        private TaskCompletionSource<bool> _initialStateTcs =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public OnlineBattleService(string sessionId, int playerId, string serverBaseUrl)
        {
            _sessionId = sessionId;
            _playerId = playerId;

            _connection = new HubConnectionBuilder()
                .WithUrl($"{serverBaseUrl}/hubs/battle")
                .WithAutomaticReconnect()
                .Build();

            RegisterHandlers();

            _connection.Reconnecting += error =>
            {
                OnConnectionStatusChanged?.Invoke(OnlineConnectionStatus.Reconnecting);

                if (error != null)
                    NotifyError(error);

                return Task.CompletedTask;
            };

            _connection.Reconnected += async _ =>
            {
                try
                {
                    await JoinSessionAsync();
                    OnConnectionStatusChanged?.Invoke(OnlineConnectionStatus.Connected);
                }
                catch (Exception ex)
                {
                    NotifyError(ex);
                    OnConnectionStatusChanged?.Invoke(OnlineConnectionStatus.Disconnected);
                }
            };

            _connection.Closed += error =>
            {
                OnConnectionStatusChanged?.Invoke(OnlineConnectionStatus.Disconnected);

                if (error != null)
                    NotifyError(error);

                return Task.CompletedTask;
            };
        }

        private void RegisterHandlers()
        {
            _connection.On<BattleSnapshot>("StateUpdated", snapshot =>
            {
                _lastSnapshot = snapshot;

                _initialStateTcs.TrySetResult(true);

                OnStateUpdated?.Invoke();
            });

            _connection.On<string>("Error", message =>
            {
                NotifyError(new InvalidOperationException(message));
            });
        }
        public async Task WaitForInitialStateAsync(int timeoutMs = 10000)
        {
            if (HasInitialState)
                return;

            Task completedTask = await Task.WhenAny(
                _initialStateTcs.Task,
                Task.Delay(timeoutMs));

            if (completedTask != _initialStateTcs.Task)
            {
                throw new TimeoutException(
                    "Battle initial state was not received from the server.");
            }

            await _initialStateTcs.Task;
        }

        public async Task ConnectAsync()
        {
            if (_connectTask != null)
            {
                await _connectTask;
                return;
            }

            _connectTask = ConnectCoreAsync();

            try
            {
                await _connectTask;
            }
            catch
            {
                _connectTask = null;
                throw;
            }
        }

        private async Task ConnectCoreAsync()
        {
            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                    await _connection.StartAsync();

                await JoinSessionAsync();
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                throw;
            }
        }

        private async Task JoinSessionAsync()
        {
            await _connection.InvokeAsync("JoinSession", _sessionId, _playerId);
        }

        public Task RunMoveAsync(int moveIndex)
        {
            return RunTurnAsync(moveIndex, OnlineBattleActionTypes.Move);
        }

        public Task RunSwitchAsync(int slotIndex)
        {
            return RunTurnAsync(slotIndex, OnlineBattleActionTypes.Switch);
        }

        public async Task RunTurnAsync(
            int index,
            string action = OnlineBattleActionTypes.Move)
        {
            if (IsOver)
                return;

            if (!OnlineBattleActionTypes.IsValid(action))
                throw new ArgumentException($"Invalid battle action type: {action}", nameof(action));

            await EnsureConnectedAsync();

            try
            {
                await _connection.InvokeAsync("SendAction", new BattleActionMessage
                {
                    SessionId = _sessionId,
                    PlayerId = _playerId,
                    ActionType = action,
                    Index = index
                });
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                throw;
            }
        }

        public async Task ForfeitAsync()
        {
            if (IsOver)
                return;

            await EnsureConnectedAsync();

            try
            {
                await _connection.InvokeAsync("Forfeit", _sessionId, _playerId);
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                throw;
            }
        }

        public BattleSnapshot GetState()
        {
            if (_lastSnapshot == null)
            {
                throw new InvalidOperationException(
                    "Battle state is not ready yet. Wait until the first StateUpdated message arrives.");
            }

            return _lastSnapshot;
        }

        public async Task DisconnectAsync()
        {
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

        private async Task EnsureConnectedAsync()
        {
            if (_connection.State == HubConnectionState.Connected)
                return;

            await ConnectAsync();
        }

        private void NotifyError(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OnlineBattleService] {ex.Message}");
            OnError?.Invoke(ex);
        }
    }

    public class BattleActionMessage
    {
        public string SessionId { get; set; } = string.Empty;
        public int PlayerId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public int Index { get; set; }
    }
}