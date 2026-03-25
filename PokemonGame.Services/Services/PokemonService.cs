using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.PokemonData;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public interface IPokemonService
    {
        PokemonLoadResult? GetPokemon(int pokemonId);
        List<PokemonLoadResult> LoadTeamResults(int battlePlayerId);
    }

    public class PokemonLoadResult
    {
        public BattlerPokemon Battler { get; set; } = null!;
        public PokemonGeneral General { get; set; } = null!;
        public PokemonStatsData Stats { get; set; } = null!;
        public List<string> MoveNames { get; set; } = new();
    }

    public class PokemonService : IPokemonService
    {
        private readonly BattlerPokemonRepository _battlerRepo;
        private readonly PokemonRepository _pokemonRepo;
        private readonly TeamRepository _teamRepo;
        private readonly TeamMemberRepository _memberRepo;

        public PokemonService()
        {
            _battlerRepo = ServiceFactory.Instance.BattlerPokemonRepository;
            _pokemonRepo = ServiceFactory.Instance.PokemonRepository;
            _teamRepo = ServiceFactory.Instance.TeamRepository;
            _memberRepo = ServiceFactory.Instance.TeamMemberRepository;
        }

        public List<PokemonLoadResult> LoadTeamResults(int battlePlayerId)
        {
            var team = _teamRepo.GetTeamByBattlePlayer(battlePlayerId)
                ?? throw new InvalidOperationException($"No team found for player {battlePlayerId}.");

            var members = _memberRepo.GetTeamMembers(team.Id);

            // Fetch and coordinate data for every member in the team
            return members
                .OrderBy(m => m.Slot_number)
                .Select(m => GetPokemon(m.PokemonID)
                    ?? throw new InvalidOperationException($"Member ID {m.PokemonID} data missing."))
                .ToList();
        }
        public List<PokemonLoadResult> GenerateRandomTeam(int count = 6, int level = 50)
        {
            // 1. Get all available Pokedex IDs from the database
            var allIds = _pokemonRepo.GetAllPokedexIds(); // You'll need this simple query in PokemonRepository
            var random = new Random();
            var results = new List<PokemonLoadResult>();

            for (int i = 0; i < count; i++)
            {
                int randomPokedexId = allIds[random.Next(allIds.Count)];

                // 2. Create a "Fake" BattlerPokemon (in-memory only)
                var randomBattler = new BattlerPokemon
                {
                    PokedexID = randomPokedexId,
                    Level = level,
                    Iv_hp = 31,
                    Iv_atk = 31,
                    Iv_def = 31,
                    Iv_spAtk = 31,
                    Iv_spDef = 31,
                    Iv_speed = 31,
                    Nature = "Hardy", // Or randomize this too
                    Move1ID = _pokemonRepo.GetRandomMoveIdForPokemon(randomPokedexId) // Helper to get a legal move
                };

                // 3. Use your existing GetPokemon logic to fill the rest
                var data = GetPokemonFromInstance(randomBattler);
                results.Add(data);
            }

            return results;
        }
        public PokemonLoadResult? GetPokemon(int pokemonId)
        {
            var battler = _battlerRepo.GetPokemonInstance(pokemonId);
            if (battler == null) return null;

            // Use the new helper!
            return GetPokemonFromInstance(battler);
        }
        // Inside PokemonService.cs

        // 3. The missing helper that fills data for a Battler object
        public PokemonLoadResult GetPokemonFromInstance(BattlerPokemon battler)
        {
            var general = _pokemonRepo.GetPokemonById(battler.PokedexID)
                ?? throw new InvalidOperationException($"No general data for PokedexID {battler.PokedexID}.");

            var stats = _pokemonRepo.GetStatsById(battler.PokedexID)
                ?? throw new InvalidOperationException($"No base stats for PokedexID {battler.PokedexID}.");

            var moveNames = GetMoveIds(battler)
                .Select(id => _pokemonRepo.GetMoveName(id)
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
        private static IEnumerable<int> GetMoveIds(BattlerPokemon b)
        {
            yield return b.Move1ID;
            if (b.Move2ID.HasValue) yield return b.Move2ID.Value;
            if (b.Move3ID.HasValue) yield return b.Move3ID.Value;
            if (b.Move4ID.HasValue) yield return b.Move4ID.Value;
        }
    }
}