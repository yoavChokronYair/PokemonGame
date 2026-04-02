using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.PokemonData;
using PokemonGame.Services.Handler;

namespace PokemonGame.Tests
{
    // ── Fake Pokemon Service ──────────────────────────────────────────────────
    // Returns 6-slot teams built from hardcoded data — no DB required.
    // Player team  : 6 × Charizard-level attacker using HyperBeam + Thunderbolt
    // Enemy  team  : 6 × Blastoise-level tank using Tackle + Thunderbolt
    internal class FakePokemonService : IPokemonService
    {
        // Slot → move-name list
        private readonly IReadOnlyList<string> _playerMoves;
        private readonly IReadOnlyList<string> _enemyMoves;

        public FakePokemonService(
            IReadOnlyList<string> playerMoves,
            IReadOnlyList<string> enemyMoves)
        {
            _playerMoves = playerMoves;
            _enemyMoves = enemyMoves;
        }

        // LoadTeamResults is what TeamTranslator calls
        public List<PokemonLoadResult> LoadTeamResults(int battlePlayerId)
        {
            // battlePlayerId 1 = player, anything else = enemy
            bool isPlayer = battlePlayerId == 1;
            var moveNames = isPlayer ? _playerMoves : _enemyMoves;

            return Enumerable.Range(0, 6)
                .Select(i => isPlayer
                    ? BuildPlayerResult(i, moveNames)
                    : BuildEnemyResult(i, moveNames))
                .ToList();
        }

        public PokemonLoadResult? GetPokemon(int pokemonId) => null; // unused in tests

        // ── Player Pokémon: Charizard-like (Fire/Flying, high Sp.Atk + Speed) ──
        private static PokemonLoadResult BuildPlayerResult(int slot, IReadOnlyList<string> moveNames)
        {
            var battler = new BattlerPokemon
            {
                PokemonID = 100 + slot,
                PokedexID = 6,   // Charizard
                Level = 50,
                AbilityID = 2,   // ID for "Blaze"
                Nature = "Timid",
                Iv_hp = 31,
                Iv_atk = 31,
                Iv_def = 31,
                Iv_spAtk = 31,
                Iv_spDef = 31,
                Iv_speed = 31,
                Ev_hp = 0,
                Ev_atk = 0,
                Ev_def = 0,
                Ev_spAtk = 252,
                Ev_spDef = 0,
                Ev_speed = 252,
            };

            var general = new PokemonGeneral
            {
                PokedexID = 6,
                Name = "Charizard",
                Type1 = "Fire",
                Type2 = "Flying",
            };

            var stats = new PokemonStatsData
            {
                PokedexID = 6,
                HP = 78,
                Attack = 84,
                Defense = 78,
                SpAtk = 109,
                SpDef = 85,
                Speed = 100,
            };

            return new PokemonLoadResult
            {
                Battler = battler,
                General = general,
                Stats = stats,
                MoveNames = moveNames.ToList(),
            };
        }

        // ── Enemy Pokémon: Blastoise-like (Water, bulky, moderate Speed) ──────
        private static PokemonLoadResult BuildEnemyResult(int slot, IReadOnlyList<string> moveNames)
        {
            var battler = new BattlerPokemon
            {
                PokemonID = 200 + slot,
                PokedexID = 9,   // Blastoise
                Level = 50,
                Nature = "Calm",
                AbilityID = 3,  // ID for "Torrent"
                Iv_hp = 31,
                Iv_atk = 31,
                Iv_def = 31,
                Iv_spAtk = 31,
                Iv_spDef = 31,
                Iv_speed = 31,
                Ev_hp = 252,
                Ev_atk = 0,
                Ev_def = 0,
                Ev_spAtk = 0,
                Ev_spDef = 252,
                Ev_speed = 4,
            };

            var general = new PokemonGeneral
            {
                PokedexID = 9,
                Name = "Blastoise",
                Type1 = "Water",
                Type2 = null,
            };

            var stats = new PokemonStatsData
            {
                PokedexID = 9,
                HP = 79,
                Attack = 83,
                Defense = 100,
                SpAtk = 85,
                SpDef = 105,
                Speed = 78,
            };

            return new PokemonLoadResult
            {
                Battler = battler,
                General = general,
                Stats = stats,
                MoveNames = moveNames.ToList(),
            };
        }
    }

}
