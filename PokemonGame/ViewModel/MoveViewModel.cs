using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.ViewModel
{
    public class MoveViewModel : ViewModelBase
    {
        private string name;
        public string Name
        {
            get => name;
            set { if (name != value) { name = value; OnPropertyChanged(nameof(Name)); } }
        }

        private int maxPP;
        public int MaxPP
        {
            get => maxPP;
            set { if (maxPP != value) { maxPP = value; OnPropertyChanged(nameof(MaxPP)); } }
        }

        private int currentPP;
        public int CurrentPP
        {
            get => currentPP;
            set { if (currentPP != value) { currentPP = value; OnPropertyChanged(nameof(CurrentPP)); } }
        }

        private string type;
        public string Type
        {
            get => type;
            set { if (type != value) { type = value; OnPropertyChanged(nameof(Type)); } }
        }
    }
}
