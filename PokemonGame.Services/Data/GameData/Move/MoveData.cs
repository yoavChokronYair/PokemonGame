namespace PokemonGame.Services.Data.GameData.Move
{
    public sealed class MoveData
    {
        public string MoveName { get; set; }
        public int Type { get; set; }//should be enum in model
        public int Category { get; set; }//should be enum in model
        public sbyte Priority { get; set; }
        public byte pPTier { get; set; }
        public byte MovePower { get; set; }
        public byte MoveAccuracy { get; set; }
        public int Effect { get; set; }//should be enum in model
        public int EffectParam { get; set; }
        public int Targets { get; set; }//should be enum in model
        public int Flags { get; set; }//should be enum in model

    }
}
