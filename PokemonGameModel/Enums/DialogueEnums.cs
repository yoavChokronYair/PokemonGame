using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Model.Enums
{
    public enum TriggerType
    {
        OnApproach,
        OnTalk,
        OnDefeat,
        OnVictory
    }

    public enum DialogueNodeType
    {
        Text,
        Choice,
        Event
    }

    public enum DialogueSetType          // was: dialogueSetType  (PascalCase)
    {
        MainStory,
        SideQuest,
        NpcInteraction                   // was: NPCInteraction
    }
}
