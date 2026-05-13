namespace PokemonGame.Services.Data.GameData.User
{
    public class StoryPlayerData
    {
        public int PlayerID { get; set; }
        public int UserID { get; set; }
    }

    public class TrainerInfoData
    {
        public int PlayerID { get; set; }
        public int Id { get; set; }
        public int TrainerID { get; set; }
        public string Name { get; set; } = "";
        public int Money { get; set; }
        public string TimePlayed { get; set; } = "00:00:00";
        public int Gender { get; set; }
        public int HallOfFameDebut { get; set; }
        public int FacingDirection { get; set; }
        public string CurrentMap { get; set; } = "";
        public string LastMapVisited { get; set; } = "";
        public int PlayerLocX { get; set; }
        public int PlayerLocY { get; set; }
        public int IsSurfing { get; set; }
        public int HasRunningShoes { get; set; }
    }

    public class BadgeData
    {
        public int PlayerID { get; set; }
        public int Id { get; set; }
        public int IsObtained { get; set; }
    }

    public class BagInventoryData
    {
        public int PlayerID { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class PartyData
    {
        public int PlayerID { get; set; }
        public int Slot { get; set; }
        public int PokedexId { get; set; }
        public string Nickname { get; set; } = "";
        public int Level { get; set; }
        public int CurrentHP { get; set; }
        public int Experience { get; set; }
        public int StatusId { get; set; }
        public int IsShiny { get; set; }
    }

    public class PokedexData
    {
        public int PlayerID { get; set; }
        public int PokedexId { get; set; }
        public int Seen { get; set; }
        public int Caught { get; set; }
    }

    public class StoryFlagData
    {
        public int PlayerID { get; set; }
        public int FlagId { get; set; }
    }

    public class DefeatedTrainerData
    {
        public int PlayerID { get; set; }
        public int TrainerId { get; set; }
    }

    public class ItemTakenData
    {
        public int PlayerID { get; set; }
        public int NpcId { get; set; }
    }

    public class TradedPokemonData
    {
        public int PlayerID { get; set; }
        public int PokedexId { get; set; }
    }
    public class StoryPlayerPokemonData
    {
        public int Id { get; set; }

        public int PlayerID { get; set; }

        // Links to battler_pokemon.pokemonID
        public int BattlerPokemonId { get; set; }

        public string? Nickname { get; set; }

        public int PokemonUID { get; set; }

        public int OriginalTrainerID { get; set; }

        public string OriginalTrainerName { get; set; } = "";

        public int ObtainMethod { get; set; }

        public string ObtainedAtRoute { get; set; } = "";

        public DateTime ObtainedAt { get; set; }

        public int ObtainedAtLevel { get; set; }

        public int CaughtWithBall { get; set; }

        public string MetLocationText { get; set; } = "";

        public int Experience { get; set; }

        public string GrowthRate { get; set; } = "MediumFast";

        public int CurrentHP { get; set; }

        public int StatusId { get; set; }

        public int Friendship { get; set; }

        public int Affection { get; set; }
    }
}