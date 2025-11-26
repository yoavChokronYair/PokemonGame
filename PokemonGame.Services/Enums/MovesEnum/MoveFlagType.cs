using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Enums.MovesEnum
{
    //TODO:test trial 
    public enum MoveFlagType : ulong
    {
        None,
        /// <summary>The move's power is boosted by <see cref="PBEAbility.IronFist"/>.</summary>
        AffectedByIronFist = 1 << 0,
        AffectedByMagicCoat = 1 << 1,
        AffectedByMirrorMove = 1 << 2,
        AffectedByProtect = 1 << 3,
        /// <summary>The move's power is boosted by <see cref="PBEAbility.Reckless"/>.</summary>
        AffectedByReckless = 1 << 4,
        AffectedBySnatch = 1 << 5,
        /// <summary>The move is blocked by <see cref="PBEAbility.Soundproof"/>.</summary>
        AffectedBySoundproof = 1 << 6,
        /// <summary>The move always lands a critical hit.</summary>
        AlwaysCrit = 1 << 7,
        BlockedByGravity = 1 << 8,
        BlockedFromAssist = 1 << 9,
        BlockedFromCopycat = 1 << 10,
        BlockedFromMeFirst = 1 << 12,
        BlockedFromMetronome = 1 << 13,
        BlockedFromMimic = 1 << 14,
        BlockedFromSketch = 1 << 15,
        BlockedFromSketchWhenSuccessful = 1 << 16,
        BlockedFromSleepTalk = 1 << 17,
        /// <summary>The move removes <see cref="PBEStatus1.Frozen"/> from the user.</summary>
        DefrostsUser = 1 << 18,
        DoubleDamageAirborne = 1 << 19,
        DoubleDamageMinimized = 1 << 20,
        DoubleDamageUnderground = 1 << 21,
        DoubleDamageUnderwater = 1 << 22,
        DoubleDamageUserDefenseCurl = 1 << 23,
        /// <summary>The move has a higher chance of landing a critical hit.</summary>
        HighCritChance = 1 << 24,
        /// <summary>The move can hit <see cref="PBEStatus2.Airborne"/> targets.</summary>
        HitsAirborne = 1 << 25,
        /// <summary>The move can hit <see cref="PBEStatus2.Underground"/> targets.</summary>
        HitsUnderground = 1 << 26,
        /// <summary>The move can hit <see cref="PBEStatus2.Underwater"/> targets.</summary>
        HitsUnderwater = 1 << 27,
        /// <summary>The user makes contact with the target, causing it to take damage from the target's <see cref="PBEAbility.IronBarbs"/>, <see cref="PBEAbility.RoughSkin"/>, and <see cref="PBEItem.RockyHelmet"/>.</summary>
        MakesContact = 1 << 28,
        NeverMissHail = 1 << 29,
        NeverMissRain = 1 << 30,
        UnaffectedByGems = 1uL << 31 // TODO
    }
}
