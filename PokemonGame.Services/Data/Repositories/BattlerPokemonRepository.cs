using PokemonGame.Services.Data.ConnectionsService;
using PokemonGame.Services.Data.GameData.Pokemon;

namespace PokemonGame.Services.Data.Repositories
{
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