using PokemonGame.ViewModel.ViewModelHelper;

namespace PokemonGame.ViewModel.BattleMenu
{
    public class MoveViewModel : ViewModelBase
    {
        private string baseName;
        public string BaseName
        {
            get => baseName;
            set { if (baseName != value) { baseName = value; OnPropertyChanged(nameof(BaseName)); OnPropertyChanged(nameof(DisplayName)); } }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set { if (isSelected != value) { isSelected = value; OnPropertyChanged(nameof(IsSelected)); OnPropertyChanged(nameof(DisplayName)); } }
        }

        public string DisplayName => IsSelected ? $"> {BaseName}" : BaseName;

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
