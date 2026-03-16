namespace PokemonGame.Services.Data.GameData.Pokemon
{
    public class BattlerPokemon
    {
        // Identity
        public int PokemonID { get; set; }
        public int PokedexID { get; set; }
        public int AbilityID { get; set; }
        public int? ItemID { get; set; }

        // Aesthetic & Status
        public int Shiny { get; set; } // 0 or 1
        public string? Gender { get; set; }
        public int Level { get; set; }

        // Moves
        public int Move1ID { get; set; }
        public int? Move2ID { get; set; }
        public int? Move3ID { get; set; }
        public int? Move4ID { get; set; }

        // Individual Values (IVs)
        public int Iv_hp { get; set; }
        public int Iv_atk { get; set; }
        public int Iv_def { get; set; }
        public int Iv_spAtk { get; set; }
        public int Iv_spDef { get; set; }
        public int Iv_speed { get; set; }

        // Effort Values (EVs)
        public int Ev_hp { get; set; }
        public int Ev_atk { get; set; }
        public int Ev_def { get; set; }
        public int Ev_spAtk { get; set; }
        public int Ev_spDef { get; set; }
        public int Ev_speed { get; set; }

        // Growth
        public string? Nature { get; set; }
    }
}