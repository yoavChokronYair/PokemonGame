using System.Collections.Generic;

namespace PokemonGame.Services.Data.Items
{
    public class PokeballData
    {
        public string Id {  get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float CatchRateModifier { get; set; }
        public bool IsGuaranteedCatch { get; set; }
        public int Price { get; set; }
        public string Sprite {  get; set; }
        public string Animation { get; set; }
        public string SoundEffect { get; set; }
        public bool CanBeUsedInBattle { get; set; }
        public bool CanBeUsedOutsideBattle { get; set; }
        public bool IsConsumabl { get; set; }
        public int UnlockLevel { get; set; }
        public string Rarity { get; set; }
    }
    public class PokeBallDataList
    {
        public List<PokeballData> Pokeballs;
    }
}
