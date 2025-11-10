using PokemonGame.Services.Enums.MovesEnum;
using PokemonGame.Services.Enums.PokemonEnum;


namespace PokemonGame.Services.Data.Move
{
    public sealed class MovesData
    {
        private string moveName;
        private PokemonType type;
        private MovesCategoryType category;
        private sbyte priority;
        private byte pPTier;
        private byte movePower;
        private byte moveAccuracy;
        private MoveEffectType effect;
        private int effectParam;
        private MoveTargetType targets;
        private MoveFlag flags;
        public string MoveName { get => moveName; set => moveName = value; }

        public PokemonType Type { get => type; set => type = value; }
        public MovesCategoryType Category { get => category; set => category = value; }
        public sbyte Priority { get => priority; set => priority = value; }
        public byte PPTier { get => pPTier; set => pPTier = value; }
        public byte MovePower { get => movePower; set => movePower = value; }
        public byte MoveAccuracy { get => moveAccuracy; set => moveAccuracy = value; }
        public MoveEffectType Effect { get => effect; set => effect = value; }
        public int EffectParam { get => effectParam; set => effectParam = value; }
        public MoveTargetType Targets { get => targets; set => targets = value; }
        public MoveFlag Flags { get => flags; set => flags = value; }
    }
}
