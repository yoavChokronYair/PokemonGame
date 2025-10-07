using PokemonGame.Model.PokemonCreation;

namespace PokemonGame.Model.Data.NpcData
{
    public class RivalData
    {
        public string? Id { get; set; }
        public string Sprite { get; set; }
        public string BattleTheme { get; set; }
        public Dialogue? Dialogue { get; set; }
        public List<TrainerPokemon> Team { get; set; }
        public Reward Rewards { get; set; }
        public bool RematchAvailable { get; set; }
    }
    public class TrainerPokemon
    {
        public int PokedexID { get; set; }
        public int Level { get; set; }
        public int MaxHP { get; set; }
        public string? Ability { get; set; }
        public string[]? Types { get; set; }
        public StatValues? IVs { get; set; }
        public StatValues? EVs { get; set; }
        public List<string>? Moves { get; set; }
    }

    public class Dialogue
    {
        public string? PreBattle { get; set; }
        public string? PostBattleWin { get; set; }
        public string? PostBattleLoss { get; set; }
    }
    public class Reward
    {
        public int Money { get; set; }
        public List<string> Items { get; set; }
        public string UnlockEvent { get; set; }
    }
    public class RivalDataList
    {
        public List<RivalData> Rivals { get; set; }
    }

}
