using PokemonGame.Interface;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.Domain.Battle
{
    public enum Weather { None, Sun, Rain, Sandstorm, Hail, HeavyRain, HarshSunlight, StrongWinds }
    public enum BattleSide { Attacker, Defender }
    public enum Screen { Reflect, LightScreen, AuroraVeil }
    public enum Hazard { Spikes, ToxicSpikes, StealthRock, StickyWeb }
    public enum Stat { Attack, Defense, SpecialAttack, SpecialDefense, Speed, Accuracy, Evasion }
    public enum StatusCondition { None, Paralysis, Burn, Poison, Toxic, Sleep, Freeze }
    public enum VolatileStatus { Confusion, Flinch, Infatuation, Curse, LeechSeed }

    // ── Side State (screens, hazards, etc.) ───────────────────────────────────

    internal class BattleSideState
    {
        private readonly Dictionary<Screen, int> screens = new();         // screen → turns remaining
        private readonly Dictionary<Hazard, int> hazards = new();         // hazard → layer count
        public bool IsSafeguardActive { get; private set; }
        public int SafeguardTurns { get; private set; }
        public bool IsMistActive { get; private set; }
        public int MistTurns { get; private set; }

        public void ActivateScreen(Screen screen, int turns)
            => screens[screen] = turns;

        public bool IsScreenActive(Screen screen)
            => screens.TryGetValue(screen, out int t) && t > 0;

        public void AddHazard(Hazard hazard)
        {
            hazards.TryGetValue(hazard, out int layers);
            hazards[hazard] = layers + 1;
        }

        public int GetHazardLayers(Hazard hazard)
            => hazards.TryGetValue(hazard, out int layers) ? layers : 0;

        public void RemoveHazard(Hazard hazard)
            => hazards.Remove(hazard);

        public void ActivateSafeguard(int turns = 5) { IsSafeguardActive = true; SafeguardTurns = turns; }
        public void ActivateMist(int turns = 5) { IsMistActive = true; MistTurns = turns; }

        // Called at end of each turn
        public void Tick()
        {
            foreach (var key in screens.Keys.ToList())
            {
                screens[key]--;
                if (screens[key] <= 0) screens.Remove(key);
            }
            if (IsSafeguardActive && --SafeguardTurns <= 0) IsSafeguardActive = false;
            if (IsMistActive && --MistTurns <= 0) IsMistActive = false;
        }
    }

    // ── Battle Domain ─────────────────────────────────────────────────────────

    internal class BattleDomain
    {
        // Active battlers
        public PokemonDomain Attacker { get; private set; }
        public PokemonDomain Defender { get; private set; }

        // Alias — IEffect/INumber classes refer to "user" and "opponent"
        public PokemonDomain ActiveUser => Attacker;
        public PokemonDomain ActiveOpponent => Defender;
        public PokemonType? ActiveTypeOverride { get; set; } = null;


        // Side states (hazards, screens)
        public BattleSideState AttackerSide { get; } = new();
        public BattleSideState DefenderSide { get; } = new();

        // Weather
        public Weather CurrentWeather { get; private set; } = Weather.None;
        public int WeatherTurnsRemaining { get; private set; } = 0;

        // Turn tracking
        public int TurnNumber { get; private set; } = 0;
        public IMove? LastUsedMove { get; private set; }

        // Damage dealt this turn — used by Drain, Bide, Parting Shot, etc.
        public int LastDamageDealt { get; set; } = 0;

        // Battle log — useful for UI and debugging
        private readonly List<string> battleLog = new();
        public IReadOnlyList<string> BattleLog => battleLog;

        public BattleDomain(PokemonDomain attacker, PokemonDomain defender)
        {
            Attacker = attacker;
            Defender = defender;
        }

        // ── Side Access ───────────────────────────────────────────────────────

        public BattleSideState GetSide(BattleSide side)
            => side == BattleSide.Attacker ? AttackerSide : DefenderSide;

        public BattleSideState GetUserSide() => AttackerSide;
        public BattleSideState GetOpponentSide() => DefenderSide;

        // ── Turn Flow ─────────────────────────────────────────────────────────

        public void BeginTurn()
        {
            TurnNumber++;
            LastDamageDealt = 0;
            Log($"--- Turn {TurnNumber} ---");
        }

        public void EndTurn()
        {
            TickWeather();
            AttackerSide.Tick();
            DefenderSide.Tick();
            ApplyEndOfTurnStatus(Attacker);
            ApplyEndOfTurnStatus(Defender);
            if (Attacker.IsBiding) Attacker.DecrementBide();
            if (Defender.IsBiding) Defender.DecrementBide();

        }

        public void SwitchAttackerDefender()
        {
            (Attacker, Defender) = (Defender, Attacker);
            // Note: sides stay tied to their trainer slot, not the active position
        }

        public void RegisterMove(IMove move)
            => LastUsedMove = move;

        // ── Weather ───────────────────────────────────────────────────────────

        public void SetWeather(Weather weather, int turns = 5)
        {
            CurrentWeather = weather;
            WeatherTurnsRemaining = turns;
            Log($"The weather changed to {weather}.");
        }

        public bool IsWeatherActive(Weather weather)
            => CurrentWeather == weather && WeatherTurnsRemaining > 0;

        private void TickWeather()
        {
            if (CurrentWeather == Weather.None) return;

            WeatherTurnsRemaining--;
            if (WeatherTurnsRemaining <= 0)
            {
                Log($"The {CurrentWeather} subsided.");
                CurrentWeather = Weather.None;
                return;
            }

            // End-of-turn weather damage
            if (CurrentWeather == Weather.Sandstorm)
            {
                ApplyWeatherDamage(Attacker, "sandstorm");
                ApplyWeatherDamage(Defender, "sandstorm");
            }
            else if (CurrentWeather == Weather.Hail)
            {
                ApplyWeatherDamage(Attacker, "hail");
                ApplyWeatherDamage(Defender, "hail");
            }
        }

        private void ApplyWeatherDamage(PokemonDomain pokemon, string source)
        {
            // Rock-types immune to sand, Ice-types immune to hail
            bool immune = source == "sandstorm"
                ? pokemon.HasType(PokemonType.Rock) || pokemon.HasType(PokemonType.Steel) || pokemon.HasType(PokemonType.Ground)
                : pokemon.HasType(PokemonType.Ice);

            if (!immune)
            {
                int dmg = pokemon.MaxHP / 16;
                pokemon.TakeDamage(dmg);
                Log($"{pokemon.Name} is buffeted by the {source}!");
            }
        }

        // ── End-of-Turn Status Damage ─────────────────────────────────────────

        private void ApplyEndOfTurnStatus(PokemonDomain pokemon)
        {
            switch (pokemon.Status)
            {
                case StatusCondition.Burn:
                    pokemon.TakeDamage(pokemon.MaxHP / 8);
                    Log($"{pokemon.Name} is hurt by its burn!");
                    break;

                case StatusCondition.Poison:
                    pokemon.TakeDamage(pokemon.MaxHP / 8);
                    Log($"{pokemon.Name} is hurt by poison!");
                    break;

                case StatusCondition.Toxic:
                    pokemon.ToxicCounter++;
                    pokemon.TakeDamage(pokemon.MaxHP * pokemon.ToxicCounter / 16);
                    Log($"{pokemon.Name} is hurt by bad poison!");
                    break;
            }
        }

        // ── Speed & Priority ──────────────────────────────────────────────────

        // Returns true if attacker moves first given priority levels
        public bool AttackerMovesFirst(int attackerPriority, int defenderPriority)
        {
            if (attackerPriority != defenderPriority)
                return attackerPriority > defenderPriority;

            int attackerSpeed = Attacker.GetEffectiveStat(Stat.Speed);
            int defenderSpeed = Defender.GetEffectiveStat(Stat.Speed);

            if (attackerSpeed != defenderSpeed)
                return attackerSpeed > defenderSpeed;

            // Speed tie — random
            return new Random().Next(2) == 0;
        }

        // ── Logging ───────────────────────────────────────────────────────────

        public void Log(string message)
        {
            battleLog.Add(message);
        }

        // ── Win Condition ─────────────────────────────────────────────────────

        public bool IsBattleOver
            => Attacker.IsFainted || Defender.IsFainted;

        public PokemonDomain? Winner
        {
            get
            {
                if (Defender.IsFainted) return Attacker;
                if (Attacker.IsFainted) return Defender;
                return null;
            }
        }
    }
}