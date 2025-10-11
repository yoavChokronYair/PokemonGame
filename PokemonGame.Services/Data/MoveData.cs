using PokemonGame.Enums;
using System.Collections.Generic;

namespace PokemonGame.Services.Data
{
    public class MoveData 
    {
        public string ename { get; set; }
        public int Power { get; set; }
        public int PP { get; set; }
        public PokemonType Type { get; set; }
        public int Accuracy { get; set; }
        public string Category { get; set; } // original value like "物理"
        public string CategoryEn
        {
            get
            {
                switch (Category)
                {
                    case "物理":
                        return "Physical";
                    case "特殊":
                        return "Special";
                    case "変化":
                        return "Status";
                    case "变化":
                        return "switch";
                    default:
                        return "Unknown";
                }
            }
        }
        public int Priority { get; set; }
    }
    public class MoveDataList
    {
        public List<MoveData> Moves { get; set; } // List of MoveData objects
    }
}