// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.

using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.DesignPatterns;

namespace PokemonGame.Model.Model.Battle
{
    public class BattleWeatherService
    {
        private readonly BattleState _battle;
        private readonly BattleLogger _logger;

        public Weather CurrentWeather { get; private set; } = Weather.None;
        public int WeatherTurnsRemaining { get; private set; } = 0;

        public BattleWeatherService(BattleState battle, BattleLogger logger)
        {
            _battle = battle;
            _logger = logger;
        }

        public void SetWeather(Weather weather, int turns = 5)
        {
            CurrentWeather = weather;
            WeatherTurnsRemaining = turns;
            _logger.Log($"The weather changed to {weather}.");
        }

        public void TickWeather()
        {
            if (CurrentWeather == Weather.None)
            {
                return;
            }

            WeatherTurnsRemaining--;
            if (WeatherTurnsRemaining <= 0)
            {
                _logger.Log($"The {CurrentWeather} subsided.");
                CurrentWeather = Weather.None;
                return;
            }

            if (CurrentWeather == Weather.Sandstorm)
            {
                ApplyWeatherDamage("sandstorm");
            }
            else if (CurrentWeather == Weather.Hail)
            {
                ApplyWeatherDamage("hail");
            }
        }
        public bool IsWeatherActive(Weather weather) => CurrentWeather == weather && WeatherTurnsRemaining > 0;
        private void ApplyWeatherDamage(string source)
        {
            if (SuppressWeather.IsWeatherSuppressed(_battle)) return;

            foreach (var p in new[] { _battle.Attacker, _battle.Defender })
            {
                bool immune = source == "sandstorm"
                    ? p.HasType(PokemonType.Rock) || p.HasType(PokemonType.Steel) || p.HasType(PokemonType.Ground)
                    : p.HasType(PokemonType.Ice);

                if (!immune && !BlockIndirectDamage.IsActive(_battle, p))
                {
                    int dmg = p.MaxHP / 16;
                    p.TakeDamage(dmg);
                    _logger.Log($"{p.Name} is buffeted by the {source}!");
                }
            }
        }
    }
}