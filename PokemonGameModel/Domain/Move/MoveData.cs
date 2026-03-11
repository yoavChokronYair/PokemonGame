// Design: Data Transfer Object / struct-like record.
// Layer: Domain — raw move data mapped from SQLite (name, type, power, accuracy, flags, etc.).
﻿using PokemonGame.Services.Enums.MovesEnum;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.Domain.Move
{
    public class MoveData
    {
        public string MoveName { get; set; }
        public PokemonType Type { get; set; }
        public MovesCategoryType Category { get; set; }
        public sbyte Priority { get; set; }
        public byte PPTier { get; set; }
        public byte MovePower { get; set; }
        public byte MoveAccuracy { get; set; }
        public MoveEffectType Effect { get; set; }
        public int EffectParam { get; set; }
        public MoveTargetType Targets { get; set; }
        public MoveFlagType Flags { get; set; }
    }
}