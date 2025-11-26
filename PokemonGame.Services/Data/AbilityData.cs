using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data
{
    public sealed class AbilityData
    {
        private int abilityID;
        private string abilityName;
        private string abilityDescription;
        private AbilityCategoryType category;
        public int AbilityID { get => abilityID; set => abilityID = value; }
        public string AbilityName { get => abilityName; set => abilityName = value; }
        public string AbilityDescription { get => abilityDescription; set => abilityDescription = value; }
        public AbilityCategoryType Category { get => category; set => category = value; }
    }
}
