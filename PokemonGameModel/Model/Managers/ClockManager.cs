using System.Timers;
using PokemonGame.Model.Config;

namespace PokemonGame.Model.Model.Managers
{
    public sealed class ClockManager: IDisposable
    {
        private static readonly Lazy<ClockManager> _instance =
            new Lazy<ClockManager>(() => new ClockManager());

        public static ClockManager Instance => _instance.Value;
        // ── Events ────────────────────────────────────────────────────────────
        /// Fired every NpcTickInterval. Subscribers: MapManager NPC logic.
        public event EventHandler? NpcTick;

        /// Fired every AutoSaveInterval. Subscriber: save system.
        public event EventHandler? AutoSave;

        /// Fired every NpcTickInterval with updated play time.
        public event EventHandler<TimeSpan>? TimePlayedUpdated;
        private readonly System.Timers.Timer _playerTimer;
        public event EventHandler? PlayerTick;

        // In constructor, after _timer setup:


        // ── State ─────────────────────────────────────────────────────────────
        public TimeSpan TimePlayed { get; private set; }
        public bool IsRunning { get; private set; }

        // ── Internals ─────────────────────────────────────────────────────────
        private readonly System.Timers.Timer _timer;
        private DateTime _lastAutoSave;
        private bool _disposed;

        // ── Construction ──────────────────────────────────────────────────────
        private ClockManager() // private constructor prevents external instantiation
        {
            _timer = new System.Timers.Timer(TimingConfig.NpcTickInterval.TotalMilliseconds)
            {
                AutoReset = true
            };
            _timer.Elapsed += OnElapsed;
            _playerTimer = new System.Timers.Timer(200) { AutoReset = true };
            _playerTimer.Elapsed += (s,e) => PlayerTick?.Invoke(this, EventArgs.Empty);;

        }



        // ── Control ───────────────────────────────────────────────────────────
        public void Start()
        {
            if (IsRunning) return;
            _lastAutoSave = DateTime.UtcNow;
            IsRunning = true;
            _timer.Start();
            _playerTimer.Start();

        }

        public void Stop()
        {
            IsRunning = false;
            _timer.Stop();
            _playerTimer.Stop();
        }

        /// Pause (e.g. dialogue open) — time stops accumulating.
        public void Pause() => Stop();
        public void Resume() => Start();

        // ── Tick ──────────────────────────────────────────────────────────────
        private void OnElapsed(object? sender, ElapsedEventArgs e)
        {
            TimePlayed += TimingConfig.NpcTickInterval;
            TimePlayedUpdated?.Invoke(this, TimePlayed);

            NpcTick?.Invoke(this, EventArgs.Empty);

            if (DateTime.UtcNow - _lastAutoSave >= TimingConfig.AutoSaveInterval)
            {
                _lastAutoSave = DateTime.UtcNow;
                AutoSave?.Invoke(this, EventArgs.Empty);
            }
        }

        // ── IDisposable ───────────────────────────────────────────────────────
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _timer.Dispose();
            _playerTimer.Dispose();
        }
    }
}

