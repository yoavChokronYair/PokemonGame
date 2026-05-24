using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Data.Repositories
{
    internal class StoryPlayerPokemonRepository : DbRepository<int, StoryPlayerPokemonData>
    {
        internal StoryPlayerPokemonRepository(IDbConnectionService db) : base(db) { }

        public List<StoryPlayerPokemonData> LoadAll(int playerID) =>
            GetAllCached(() =>
                _db.Query<StoryPlayerPokemonData>(
                    @"SELECT * 
                  FROM StoryPlayerPokemon 
                  WHERE PlayerID = @PlayerID",
                    new { PlayerID = playerID }),
                p => p.Id);

        public StoryPlayerPokemonData? Load(int id) =>
            GetCached(id, () =>
                _db.QuerySingle<StoryPlayerPokemonData>(
                    @"SELECT * 
                  FROM StoryPlayerPokemon 
                  WHERE Id = @Id",
                    new { Id = id }));

        public void Save(StoryPlayerPokemonData pokemon)
        {
            pokemon.BattlerPokemonId = EnsureBattlerPokemonExists(pokemon);

            _db.Execute(@"
                INSERT OR REPLACE INTO StoryPlayerPokemon
                (
                    PlayerID,
                    BattlerPokemonId,
                    Nickname,
                    PokemonUID,
                    OriginalTrainerID,
                    OriginalTrainerName,
                    ObtainMethod,
                    ObtainedAtRoute,
                    ObtainedAt,
                    ObtainedAtLevel,
                    CaughtWithBall,
                    MetLocationText,
                    Experience,
                    GrowthRate,
                    CurrentHP,
                    StatusId,
                    Friendship,
                    Affection
                )
                VALUES
                (
                    @PlayerID,
                    @BattlerPokemonId,
                    @Nickname,
                    @PokemonUID,
                    @OriginalTrainerID,
                    @OriginalTrainerName,
                    @ObtainMethod,
                    @ObtainedAtRoute,
                    @ObtainedAt,
                    @ObtainedAtLevel,
                    @CaughtWithBall,
                    @MetLocationText,
                    @Experience,
                    @GrowthRate,
                    @CurrentHP,
                    @StatusId,
                    @Friendship,
                    @Affection
                )",
                new
                {
                    pokemon.PlayerID,
                    pokemon.BattlerPokemonId,
                    pokemon.Nickname,
                    pokemon.PokemonUID,
                    pokemon.OriginalTrainerID,
                    pokemon.OriginalTrainerName,
                    pokemon.ObtainMethod,
                    pokemon.ObtainedAtRoute,
                    ObtainedAt = pokemon.ObtainedAt.ToString("o"),
                    pokemon.ObtainedAtLevel,
                    pokemon.CaughtWithBall,
                    pokemon.MetLocationText,
                    pokemon.Experience,
                    pokemon.GrowthRate,
                    pokemon.CurrentHP,
                    pokemon.StatusId,
                    pokemon.Friendship,
                    pokemon.Affection
                });

            StoreAndReturn(pokemon.Id, () => pokemon);
        }
        private int EnsureBattlerPokemonExists(StoryPlayerPokemonData pokemon)
        {
            // 1. If BattlerPokemonId already points to a real battler_pokemon row,
            // keep it.
            int existingInstance = _db.QueryScalar<int>(
                @"SELECT COUNT(*)
                FROM battler_pokemon
                WHERE pokemonID = @id",
                new { id = pokemon.BattlerPokemonId });

            if (existingInstance > 0)
                return pokemon.BattlerPokemonId;

            // 2. If it does not exist, treat the current value as a PokedexID.
            // Example: 25 means Pikachu species.
            int pokedexId = pokemon.BattlerPokemonId;

            int speciesExists = _db.QueryScalar<int>(
                @"SELECT COUNT(*)
                FROM pokemon_general
                WHERE pokedexID = @pokedexId",
                new { pokedexId });

            if (speciesExists == 0)
            {
                throw new InvalidOperationException(
                    $"Cannot create battler_pokemon. Value {pokedexId} is neither an existing pokemonID nor a valid pokedexID.");
            }

            int abilityId = _db.QueryScalar<int>(
                @"SELECT COALESCE(firstAbilityID, secondAbilityID, hiddenAbilityID)
                FROM pokemon_general
                WHERE pokedexID = @pokedexId",
                new { pokedexId });

            if (abilityId <= 0)
            {
                throw new InvalidOperationException(
                    $"Cannot create battler_pokemon for PokedexID {pokedexId}: no ability found.");
            }

            var moveIds = _db.QueryScalarList<int>(
                @"SELECT moveID
                FROM levelup_moves
                WHERE pokedexID = @pokedexId
                AND level <= @level
                ORDER BY level DESC, moveID ASC
                LIMIT 4",
                new
                {
                    pokedexId,
                    level = pokemon.ObtainedAtLevel <= 0 ? 1 : pokemon.ObtainedAtLevel
                });

            if (moveIds.Count == 0)
            {
                moveIds = _db.QueryScalarList<int>(
                    @"SELECT moveID
              FROM levelup_moves
              WHERE pokedexID = @pokedexId
              ORDER BY level ASC, moveID ASC
              LIMIT 4",
                    new { pokedexId });
            }

            if (moveIds.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Cannot create battler_pokemon for PokedexID {pokedexId}: no moves found.");
            }

            while (moveIds.Count < 4)
                moveIds.Add(moveIds[0]);

            int level = pokemon.ObtainedAtLevel <= 0 ? 1 : pokemon.ObtainedAtLevel;

            int newBattlerPokemonId = _db.ExecuteAndGetLastId(@"
                INSERT INTO battler_pokemon
                (
                    pokedexID,
                    abilityID,
                    itemID,
                    shiny,
                    gender,
                    level,
                    move1ID,
                    move2ID,
                    move3ID,
                    move4ID,
                    iv_hp,
                    iv_atk,
                    iv_def,
                    iv_spAtk,
                    iv_spDef,
                    iv_speed,
                    ev_hp,
                    ev_atk,
                    ev_def,
                    ev_spAtk,
                    ev_spDef,
                    ev_speed,
                    nature
                )
                VALUES
                (
                    @PokedexID,
                    @AbilityID,
                    NULL,
                    0,
                    NULL,
                    @Level,
                    @Move1ID,
                    @Move2ID,
                    @Move3ID,
                    @Move4ID,
                    31,
                    31,
                    31,
                    31,
                    31,
                    31,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    'Hardy'
                );",
                new
                {
                    PokedexID = pokedexId,
                    AbilityID = abilityId,
                    Level = level,
                    Move1ID = moveIds[0],
                    Move2ID = moveIds[1],
                    Move3ID = moveIds[2],
                    Move4ID = moveIds[3],
                });

            System.Diagnostics.Debug.WriteLine(
                $"Created missing battler_pokemon row. PokedexID={pokedexId}, New pokemonID={newBattlerPokemonId}");

            return newBattlerPokemonId;
        }
        public void SaveAll(IEnumerable<StoryPlayerPokemonData> pokemon)
        {
            foreach (var p in pokemon)
                Save(p);
        }

        public void Delete(int id) =>
            _db.Execute(
                "DELETE FROM StoryPlayerPokemon WHERE Id = @Id",
                new { Id = id });

        public void Clear(int playerID) =>
            _db.Execute(
                "DELETE FROM StoryPlayerPokemon WHERE PlayerID = @PlayerID",
                new { PlayerID = playerID });
    }

    internal class BattlerPokemonRepository
    {
        private readonly IDbConnectionService _db;

        internal BattlerPokemonRepository(IDbConnectionService db) => _db = db;

        // Fetch a specific instance of a Pokemon
        public BattlerPokemon? GetPokemonInstance(int pokemonID) =>
            _db.QuerySingle<BattlerPokemon>(
                "SELECT * FROM battler_pokemon WHERE pokemonID = @pid",
                new { pid = pokemonID });

        // Add a new captured/bred Pokemon instance
        public int CreatePokemonInstance(BattlerPokemon pokemon)
        {

            var result = _db.ExecuteAndGetLastId(@"
        INSERT INTO battler_pokemon (
            pokedexID, abilityID, itemID, shiny, gender, level, 
            move1ID, move2ID, move3ID, move4ID, 
            iv_hp, iv_atk, iv_def, iv_spAtk, iv_spDef, iv_speed,
            ev_hp, ev_atk, ev_def, ev_spAtk, ev_spDef, ev_speed, nature
        ) VALUES (
            @PokedexID, @AbilityID, @ItemID, @Shiny, @Gender, @Level,
            @Move1ID, @Move2ID, @Move3ID, @Move4ID,
            @Iv_hp, @Iv_atk, @Iv_def, @Iv_spAtk, @Iv_spDef, @Iv_speed,
            @Ev_hp, @Ev_atk, @Ev_def, @Ev_spAtk, @Ev_spDef, @Ev_speed, @Nature
        );", pokemon);

            System.Diagnostics.Debug.WriteLine($"Created pokemon instance ID: {result}");
            return result;

        }

        // Update stats (EVs or Level) after training or leveling
        public void UpdatePokemonStats(BattlerPokemon p)
        {
            _db.Execute(@"
                UPDATE battler_pokemon 
                SET level = @Level, ev_hp = @Ev_hp, ev_atk = @Ev_atk, 
                    ev_def = @Ev_def, ev_spAtk = @Ev_spAtk, ev_spDef = @Ev_spDef, ev_speed = @Ev_speed
                WHERE pokemonID = @PokemonID", p);
        }
        public (int PokedexID, int? ItemID) GetPokemonIdentity(int instanceID)
        {
            var data = _db.QuerySingle<BattlerPokemon>(
                "SELECT pokedexID, itemID FROM battler_pokemon WHERE pokemonID = @pid",
                new { pid = instanceID });

            return (data.PokedexID, data.ItemID);
        }   

        // Delete a Pokemon (e.g., releasing it)
        public void DeletePokemonInstance(int pokemonID) =>
            _db.Execute("DELETE FROM battler_pokemon WHERE pokemonID = @pid", new { pid = pokemonID });
        public void Upsert(BattlerPokemon r)
        {
            _db.Execute(
                @"INSERT OR REPLACE INTO battler_pokemon
            (pokemonID, pokedexID, abilityID, itemID, shiny, gender, level,
             move1ID, move2ID, move3ID, move4ID,
             iv_hp, iv_atk, iv_def, iv_spAtk, iv_spDef, iv_speed,
             ev_hp, ev_atk, ev_def, ev_spAtk, ev_spDef, ev_speed,
             nature)
          VALUES
            (@pokemonID, @pokedexID, @abilityID, @itemID, @shiny, @gender, @level,
             @move1, @move2, @move3, @move4,
             @ivHp, @ivAtk, @ivDef, @ivSpAtk, @ivSpDef, @ivSpeed,
             @evHp, @evAtk, @evDef, @evSpAtk, @evSpDef, @evSpeed,
             @nature)",
                new
                {
                    pokemonID = r.PokemonID,
                    pokedexID = r.PokedexID,
                    abilityID = r.AbilityID,
                    itemID = r.ItemID,
                    shiny = r.Shiny,
                    gender = r.Gender,
                    level = r.Level,
                    move1 = r.Move1ID,
                    move2 = r.Move2ID,
                    move3 = r.Move3ID,
                    move4 = r.Move4ID,
                    ivHp = r.Iv_hp,
                    ivAtk = r.Iv_atk,
                    ivDef = r.Iv_def,
                    ivSpAtk = r.Iv_spAtk,
                    ivSpDef = r.Iv_spDef,
                    ivSpeed = r.Iv_speed,
                    evHp = r.Ev_hp,
                    evAtk = r.Ev_atk,
                    evDef = r.Ev_def,
                    evSpAtk = r.Ev_spAtk,
                    evSpDef = r.Ev_spDef,
                    evSpeed = r.Ev_speed,
                    nature = r.Nature
                });
        }
    }
}