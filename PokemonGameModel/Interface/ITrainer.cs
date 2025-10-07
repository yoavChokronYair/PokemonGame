using System;
using System.Collections.Generic;

namespace PokemonGame.Interface
{
    public interface ITrainer
    {
        // Basic Info
        string Name { get; }
        string Description { get; } // e.g., "Cooltrainer", "Bug Catcher"

        // Dialogue
        List<string> PreBattleDialog { get; } // Before battle starts
        List<string> PostBattleDialogWin { get; } // If player wins
        List<string> PostBattleDialogLose { get; } // If player loses
        List<string> MidBattleDialog { get; } // Optional: triggers mid-battle

        // Battle Setup
        int MoneyReward { get; } // Money given to player upon defeat
        bool CanRematch { get; } // If trainer can be re-battled
        bool IsDeafeted { get; } // Gym Leader, Rival, Elite Four, etc.\
        bool IsHidden {  get; }
        // Location Info
        string EncounterLocation { get; } // Route, Gym, Cave, etc.
        bool IsBattleMandatory { get; } // Must fight to progress

        // Optional Extras
        string MusicTheme { get; } // TrainerData-specific music
        string SpriteAssetKey { get; } // For trainer's sprite

        // Items or Rewards
        List<string> ItemRewards { get; } // TM, Item, etc.
    }
}
