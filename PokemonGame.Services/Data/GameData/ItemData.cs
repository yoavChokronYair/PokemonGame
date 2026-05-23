namespace PokemonGame.Services.Data.GameData
{
    // =========================================================
    // items
    // =========================================================

    public class ItemData
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Item_type { get; set; }

        public string? Category { get; set; }

        public int Price { get; set; }

        public string? Description { get; set; }

        public int Is_consumable { get; set; }

        public int Usable_in_battle { get; set; }

        public int Usable_in_field { get; set; }

        public int? Effect_id { get; set; }

        public int? Condition_id { get; set; }
    }


    // =========================================================
    // held_items
    // =========================================================

    public class HeldItemData
    {
        public int Id { get; set; }

        public int Item_id { get; set; }

        public int Effect_id { get; set; }

        public int? Condition_id { get; set; }

        public int Is_one_time_use { get; set; }

        public string? Trigger { get; set; }
    }


    // =========================================================
    // keyitems
    // =========================================================

    public class KeyItemData
    {
        public int Id { get; set; }

        public int Item_id { get; set; }

        public int? Usage_effect_id { get; set; }

        public int? Condition_id { get; set; }

        public int Registerable { get; set; }
    }


    // =========================================================
    // pokeballs
    // =========================================================

    public class PokeballData
    {
        public int Id { get; set; }

        public int Item_id { get; set; }

        public int? Caught_effect_id { get; set; }

        public int? Condition_id { get; set; }

        public float Multiplier { get; set; }

        public string? Ball_type { get; set; }
    }


    // =========================================================
    // tms_hms
    // =========================================================

    public class TmHmData
    {
        public int Id { get; set; }

        public int Item_id { get; set; }

        public int Move_id { get; set; }

        public int Is_hm { get; set; }

        public string? Machine_id { get; set; }
    }


    // =========================================================
    // player used tms
    // =========================================================

    public class PlayerUsedTmData
    {
        public int PlayerID { get; set; }

        public int TmHmId { get; set; }
    }


    // =========================================================
    // registered key items
    // =========================================================

    public class RegisteredKeyItemData
    {
        public int PlayerID { get; set; }

        public int KeyItemId { get; set; }
    }


    // =========================================================
    // npc item gifts
    // =========================================================

    public class PlayerItemGiftData
    {
        public int PlayerID { get; set; }

        public int ItemGivingId { get; set; }

        public int HasBeenGiven { get; set; }
    }
}