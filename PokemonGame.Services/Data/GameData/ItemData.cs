namespace PokemonGame.Services.Data.GameData
{
    public class ItemData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public int Price { get; set; }
        public int Is_consumable { get; set; }
        public int? Effect_id { get; set; }
        public int? Condition_id { get; set; }
    }
}
