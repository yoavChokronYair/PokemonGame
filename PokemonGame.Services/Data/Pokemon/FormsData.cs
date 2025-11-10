using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Data.Pokemon
{
    public sealed class FormsData
    {
        private string formName;
        private byte formID;
        public string FormName { get => formName; set => formName = value; }
        public byte FormID { get => formID; set => formID = value; }
    }
}
