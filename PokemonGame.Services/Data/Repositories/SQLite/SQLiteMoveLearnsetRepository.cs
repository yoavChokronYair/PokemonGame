using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Move;

namespace PokemonGame.Services.Data.Repositories.SQLite
{
    internal class SQLiteMoveLearnsetRepository
    {
        private readonly ISQLiteConnectionService _db;

        private Dictionary<int, List<LevelUpMoveData>>? _levelUpCache;
        private Dictionary<int, List<MachineMoveData>>? _machineCache;
        private Dictionary<int, List<EggMoveData>>? _eggCache;
        private Dictionary<int, List<TutorMoveData>>? _tutorCache;

        internal SQLiteMoveLearnsetRepository(ISQLiteConnectionService db) => _db = db;

        private void EnsureLoaded()
        {
            if (_levelUpCache == null)
            {
                _levelUpCache = _db.Query<LevelUpMoveData>("SELECT * FROM levelup_moves")
                                  .GroupBy(m => m.PokedexID).ToDictionary(g => g.Key, g => g.ToList());

                _machineCache = _db.Query<MachineMoveData>("SELECT * FROM machine_moves")
                                  .GroupBy(m => m.PokedexID).ToDictionary(g => g.Key, g => g.ToList());

                _eggCache = _db.Query<EggMoveData>("SELECT * FROM egg_moves")
                               .GroupBy(m => m.PokedexID).ToDictionary(g => g.Key, g => g.ToList());

                _tutorCache = _db.Query<TutorMoveData>("SELECT * FROM tutor_moves")
                                 .GroupBy(m => m.PokedexID).ToDictionary(g => g.Key, g => g.ToList());
            }
        }

        public List<LevelUpMoveData> GetLevelUpMoves(int pokedexID)
        {
            EnsureLoaded();
            return _levelUpCache!.TryGetValue(pokedexID, out var moves) ? moves : new List<LevelUpMoveData>();
        }

        public List<MachineMoveData> GetMachineMoves(int pokedexID)
        {
            EnsureLoaded();
            return _machineCache!.TryGetValue(pokedexID, out var moves) ? moves : new List<MachineMoveData>();
        }

        public List<EggMoveData> GetEggMoves(int pokedexID)
        {
            EnsureLoaded();
            return _eggCache!.TryGetValue(pokedexID, out var moves) ? moves : new List<EggMoveData>();
        }

        public List<TutorMoveData> GetTutorMoves(int pokedexID)
        {
            EnsureLoaded();
            return _tutorCache!.TryGetValue(pokedexID, out var moves) ? moves : new List<TutorMoveData>();
        }
    }
}