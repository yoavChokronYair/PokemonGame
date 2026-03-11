// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.

using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    internal class BattleWeatherService
    {
        private readonly BattleState battle;
        private readonly BattleLogger logger;

        public Weather CurrentWeather { get; private set; } = Weather.None;
        public int WeatherTurnsRemaining { get; private set; } = 0;

        public BattleWeatherService(BattleState battle, BattleLogger logger)
        {
            this.battle = battle;
            this.logger = logger;
        }

        public void SetWeather(Weather weather, int turns = 5)
        {
            CurrentWeather = weather;
            WeatherTurnsRemaining = turns;
            logger.Log($"The weather changed to {weather}.");
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
                logger.Log($"The {CurrentWeather} subsided.");
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
            foreach (var p in new[] { battle.Attacker, battle.Defender })
            {
                bool immune = source == "sandstorm"
                    ? p.HasType(PokemonType.Rock) || p.HasType(PokemonType.Steel) || p.HasType(PokemonType.Ground)
                    : p.HasType(PokemonType.Ice);

                if (!immune)
                {
                    int dmg = p.MaxHP / 16;
                    p.TakeDamage(dmg);
                    logger.Log($"{p.Name} is buffeted by the {source}!");
                }
            }
        }
    }
}