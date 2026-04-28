using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Model.Enums
{
    public enum TriggerType
    {
        Interact,
        Spotted
    }

    public enum DialogueNodeType
    {
        Text,
        Choice,
        Condition,
        Terminal//(Terminal → triggers OnDialogueFinishedTrue)
    }

    public enum DialogueSetType         
    {
        MainStory,
        SideQuest,
        NpcInteraction,           
        Trade,
        ItemGiver,
        Shop,
        Pokecenter,
        Trainer,
        GymLeader

    }
}
