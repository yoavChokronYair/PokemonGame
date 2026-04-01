namespace PokemonGame.Model.Enums
{
    public enum BattleLogPhase
    {
        Setup,        // "Go! Charizard!", intro messages
        TurnStart,    // "--- Turn 1 ---"
        Action,       // move used, damage dealt, effectiveness
        StatusEffect, // end-of-turn burn/poison damage
        Faint,        // "Charizard fainted!"
        Switch,       // "Player sends out Charizard!"
        Weather,      // weather tick messages
        BattleEnd,    // "Player wins!"
    }

}
