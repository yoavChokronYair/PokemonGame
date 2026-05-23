using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.Map;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Services.Handler
{
    public class LocalPokemonService : IPokemonService
    {
        private readonly BattlerPokemonRepository _battlerRepo;
        private readonly PokemonRepository _pokemonRepo;
        private readonly TeamRepository _teamRepo;
        private readonly TeamMemberRepository _memberRepo;
        private readonly MoveLearnsetRepository _moveLearnsetRepository;
        private readonly PokemonStatsRepository _pokemonStatsRepository;
        private readonly MoveRepository _moveRepository;

        public LocalPokemonService()
        {
            var f = ServiceFactory.Instance;
            _battlerRepo = f.BattlerPokemonRepository;
            _pokemonRepo = f.PokemonRepository;
            _teamRepo = f.TeamRepository;
            _memberRepo = f.TeamMemberRepository;
            _moveLearnsetRepository = f.MoveLearnsetRepository;
            _pokemonStatsRepository = f.PokemonStatsRepository;
            _moveRepository = f.MoveRepository;
        }

        internal LocalPokemonService(
            BattlerPokemonRepository battlerRepo,
            PokemonRepository pokemonRepo,
            TeamRepository teamRepo,
            TeamMemberRepository memberRepo,
            MoveLearnsetRepository moveLearnsetRepository,
            PokemonStatsRepository pokemonStatsRepository,
            MoveRepository moveRepository)
        {
            _battlerRepo = battlerRepo;
            _pokemonRepo = pokemonRepo;
            _teamRepo = teamRepo;
            _memberRepo = memberRepo;
            _moveLearnsetRepository = moveLearnsetRepository;
            _pokemonStatsRepository = pokemonStatsRepository;
            _moveRepository = moveRepository;
        }

        // ── All method bodies identical to your existing PokemonService ───────
        public PokemonLoadResult? LoadPokemon(int pokemonId)
        {
            var battler = _battlerRepo.GetPokemonInstance(pokemonId);
            if (battler == null) return null;
            return GetPokemonFromInstance(battler);
        }

        public PokemonLoadResult GetPokemonFromInstance(BattlerPokemon battler)
        {
            var general = _pokemonRepo.GetPokemonById(battler.PokedexID)
                ?? throw new InvalidOperationException($"No general data for PokedexID {battler.PokedexID}.");

            var stats = _pokemonStatsRepository.GetBaseStats(battler.PokedexID)
                ?? throw new InvalidOperationException($"No base stats for PokedexID {battler.PokedexID}.");

            var moveNames = GetMoveIds(battler)
                .Select(id => _moveRepository.GetName(id)
                    ?? throw new InvalidOperationException($"Move ID {id} not found."))
                .ToList();

            return new PokemonLoadResult
            {
                Battler = battler,
                General = general,
                Stats = stats,
                MoveNames = moveNames,
            };
        }

        public List<PokemonLoadResult> LoadTeamResults(int battlePlayerId)
        {
            var team = _teamRepo.GetTeamByBattlePlayer(battlePlayerId);
            if (team == null)
                throw new InvalidOperationException($"No team found for player {battlePlayerId}.");

            var members = _memberRepo.GetTeamMembers(team.Id);
            return members
                .OrderBy(m => m.Slot_number)
                .Select(m => LoadPokemon(m.PokemonID)
                    ?? throw new InvalidOperationException($"Member ID {m.PokemonID} data missing."))
                .ToList();
        }

        public List<PokemonLoadResult> GenerateRandomTeam(int count = 6, int level = 50)
        {
            var allIds = _pokemonRepo.GetAllPokedexIds();
            var random = new Random();
            var results = new List<PokemonLoadResult>();

            // Keep going until we have successfully generated the requested count
            while (results.Count < count)
            {
                try
                {
                    int randomPokedexId = allIds[random.Next(allIds.Count)];
                    var pokemonData = _pokemonRepo.GetPokemonById(randomPokedexId);

                    var abilityPool = new List<int?>
                    {
                        pokemonData.FirstAbilityID,
                        pokemonData.SecondAbilityID,
                        pokemonData.HiddenAbilityID
                    }
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToList();

                    // Guard clause in case a Pokémon somehow has zero valid abilities
                    if (abilityPool.Count == 0)
                    {
                        throw new Exception($"Pokemon ID {randomPokedexId} has no valid abilities.");
                    }

                    int randomAbilityId = abilityPool[random.Next(abilityPool.Count)];

                    var randomBattler = new BattlerPokemon
                    {
                        PokedexID = randomPokedexId,
                        Level = level,
                        AbilityID = randomAbilityId,
                        Iv_hp = 31,
                        Iv_atk = 31,
                        Iv_def = 31,
                        Iv_spAtk = 31,
                        Iv_spDef = 31,
                        Iv_speed = 31,
                        Nature = "Hardy",
                        Move1ID = _moveLearnsetRepository.GetRandomMoveIdForPokemon(randomPokedexId)
                    };

                    results.Add(GetPokemonFromInstance(randomBattler));
                }
                catch (Exception ex)
                {
                    // Log the error if you have a logger, or write to debug console
                    Console.WriteLine($"Failed to generate Pokémon. Retrying... Error: {ex.Message}");

                    // We do NOT increment or exit. 
                    // The while loop will spin again and attempt a new random Pokémon.
                }
            }

            return results;
        }

        public PokemonLoadResult GenerateWildPokemon(EncounterData encounter)
        {
            var random = new Random();

            var pokemonData = _pokemonRepo.GetPokemonById(encounter.PokemonId);
            if (pokemonData == null)
                throw new InvalidOperationException($"Pokemon {encounter.PokemonId} not found.");

            // ── Level from encounter ─────────────────────────
            int level = random.Next(encounter.MinLevel, encounter.MaxLevel + 1);

            // ── Ability selection ────────────────────────────
            var abilityPool = new List<int?>
            {
                pokemonData.FirstAbilityID,
                pokemonData.SecondAbilityID,
                pokemonData.HiddenAbilityID
            }
            .Where(a => a.HasValue)
            .Select(a => a!.Value)
            .ToList();

            if (abilityPool.Count == 0)
                throw new InvalidOperationException($"Pokemon {encounter.PokemonId} has no abilities.");

            int abilityId = abilityPool[random.Next(abilityPool.Count)];

            // ── Moves (based on level) ───────────────────────
            var learnset = _moveLearnsetRepository.GetLevelUpMoves(encounter.PokemonId);

            var moves = learnset
                .Where(m => m.Level <= level)
                .OrderBy(m => m.Level)
                .Select(m => m.MoveID)
                .Distinct()
                .Reverse()
                .Take(4)
                .ToList();

            // ── fallback moves if needed ─────────────────────
            while (moves.Count < 4)
            {
                var fallback = _moveLearnsetRepository
                    .GetRandomMoveIdForPokemon(encounter.PokemonId);

                if (!moves.Contains(fallback))
                    moves.Add(fallback);
            }

            // ── Build battler ───────────────────────────────
            var battler = new BattlerPokemon
            {
                PokedexID = encounter.PokemonId,
                Level = level,
                AbilityID = abilityId,

                Iv_hp = RandomIV(),
                Iv_atk = RandomIV(),
                Iv_def = RandomIV(),
                Iv_spAtk = RandomIV(),
                Iv_spDef = RandomIV(),
                Iv_speed = RandomIV(),

                Nature = GetRandomNature(),

                Move1ID = moves[0],
                Move2ID = moves[1],
                Move3ID = moves[2],
                Move4ID = moves[3],
            };

            return GetPokemonFromInstance(battler);
        }
        private static readonly string[] Natures =
        {
            "Hardy", "Adamant", "Modest", "Jolly", "Timid",
            "Bold", "Calm", "Impish", "Careful"
        };

        private static int RandomIV()
        {
            Random random = new Random();
            return random.Next(0, 32); // 0–31
        }

        private static string GetRandomNature()
        {
            Random random = new Random();

            return Natures[random.Next(Natures.Length)];
        }
        private static IEnumerable<int> GetMoveIds(BattlerPokemon b)
        {
            yield return b.Move1ID;
            if (b.Move2ID.HasValue) yield return b.Move2ID.Value;
            if (b.Move3ID.HasValue) yield return b.Move3ID.Value;
            if (b.Move4ID.HasValue) yield return b.Move4ID.Value;
        }
    }

}