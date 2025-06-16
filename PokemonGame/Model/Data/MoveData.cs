using PokemonGame.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Model.Data
{
    public class MoveData 
    {
        public string ename { get; set; } // maps to "ename"
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

    }
    public class MoveDataList
    {
        public List<MoveData> Moves { get; set; } // List of MoveData objects
    }
}