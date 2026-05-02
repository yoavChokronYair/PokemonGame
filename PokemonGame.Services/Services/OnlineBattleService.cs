// PokemonGame.Services/Services/OnlineBattleService.cs
// TCP client service.  The ViewModel calls the public methods;
// incoming server packets raise strongly-typed C# events.

using System.Net.Sockets;
using System.Text.Json;
using PokemonGame.Services.Network;
using PokemonGame.Services.Network.Packets;

namespace PokemonGame.Services.Handler
{
    public class OnlineBattleService : IDisposable
    {
        // ── Events ────────────────────────────────────────────────────────────
        // All events are raised on a ThreadPool thread.
        // ViewModels must marshal to the UI thread if needed.

        public event Action<MatchFoundPacket>? OnMatchFound;
        public event Action<TurnResultPacket>? OnTurnResult;
        public event Action<ForcedSwitchPacket>? OnForcedSwitch;
        public event Action<BattleEndPacket>? OnBattleEnd;
        public event Action<string>? OnError;

        // ── State ─────────────────────────────────────────────────────────────

        private TcpClient? _tcp;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;

        public bool IsConnected => _tcp?.Connected == true;
        public string CurrentRoomId { get; private set; } = string.Empty;

        // ── Connection ────────────────────────────────────────────────────────

        public async Task ConnectAsync(string host, int port)
        {
            _tcp = new TcpClient();
            await _tcp.ConnectAsync(host, port).ConfigureAwait(false);
            _stream = _tcp.GetStream();
            _cts = new CancellationTokenSource();

            // Start background receive loop
            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
        }

        public void Disconnect()
        {
            _cts?.Cancel();
            _stream?.Close();
            _tcp?.Close();
        }

        // ── Client → Server actions ───────────────────────────────────────────

        public async Task FindMatchAsync(FindMatchPacket packet)
        {
            EnsureConnected();
            await PacketHelper.WritePacketAsync(_stream!, packet).ConfigureAwait(false);
        }

        public async Task SendMoveAsync(int moveIndex)
        {
            EnsureConnected();
            await PacketHelper.WritePacketAsync(_stream!, new MoveActionPacket
            {
                RoomId = CurrentRoomId,
                MoveIndex = moveIndex
            }).ConfigureAwait(false);
        }

        public async Task SendSwitchAsync(int slotIndex)
        {
            EnsureConnected();
            await PacketHelper.WritePacketAsync(_stream!, new SwitchActionPacket
            {
                RoomId = CurrentRoomId,
                SlotIndex = slotIndex
            }).ConfigureAwait(false);
        }

        public async Task SendForfeitAsync()
        {
            EnsureConnected();
            await PacketHelper.WritePacketAsync(_stream!, new ForfeitPacket
            {
                RoomId = CurrentRoomId
            }).ConfigureAwait(false);
        }

        // ── Receive loop ──────────────────────────────────────────────────────

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    string? raw = await PacketHelper.ReadRawPacketAsync(_stream!).ConfigureAwait(false);
                    if (raw == null) break; // server closed connection

                    DispatchPacket(raw);
                }
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                OnError?.Invoke(ex.Message);
            }
        }

        private void DispatchPacket(string raw)
        {
            using var doc = JsonDocument.Parse(raw);
            string type = doc.RootElement.GetProperty("type").GetString() ?? "";

            switch (type)
            {
                case "MatchFound":
                    var mf = JsonSerializer.Deserialize<MatchFoundPacket>(raw)!;
                    CurrentRoomId = mf.RoomId;
                    OnMatchFound?.Invoke(mf);
                    break;

                case "TurnResult":
                    OnTurnResult?.Invoke(JsonSerializer.Deserialize<TurnResultPacket>(raw)!);
                    break;

                case "ForcedSwitch":
                    OnForcedSwitch?.Invoke(JsonSerializer.Deserialize<ForcedSwitchPacket>(raw)!);
                    break;

                case "BattleEnd":
                    OnBattleEnd?.Invoke(JsonSerializer.Deserialize<BattleEndPacket>(raw)!);
                    break;

                default:
                    Console.WriteLine($"[OnlineBattleService] Unknown packet type: {type}");
                    break;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void EnsureConnected()
        {
            if (_stream == null || !IsConnected)
                throw new InvalidOperationException("Not connected to the battle server.");
        }

        public void Dispose()
        {
            Disconnect();
            _cts?.Dispose();
        }
    }
}