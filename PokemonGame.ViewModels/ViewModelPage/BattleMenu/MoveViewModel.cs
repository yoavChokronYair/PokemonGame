using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class MoveViewModel : ViewModelBase
    {
        private string _baseName;
        public string BaseName
        {
            get => _baseName;
            set { if (_baseName != value) { _baseName = value; OnPropertyChanged(nameof(BaseName)); OnPropertyChanged(nameof(DisplayName)); } }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); OnPropertyChanged(nameof(DisplayName)); } }
        }

        public string DisplayName => IsSelected ? $"> {BaseName}" : BaseName;

        private int _maxPP;
        public int MaxPP
        {
            get => _maxPP;
            set { if (_maxPP != value) { _maxPP = value; OnPropertyChanged(nameof(MaxPP)); } }
        }

        private int _currentPP;
        public int CurrentPP
        {
            get => _currentPP;
            set { if (_currentPP != value) { _currentPP = value; OnPropertyChanged(nameof(CurrentPP)); } }
        }

        private string _type;
        public string Type
        {
            get => _type;
            set { if (_type != value) { _type = value; OnPropertyChanged(nameof(Type)); } }
        }


    }
}
