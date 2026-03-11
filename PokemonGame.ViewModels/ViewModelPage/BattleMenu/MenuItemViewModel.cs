using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class MenuItemViewModel : ViewModelBase
    {
        public string Label { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(FormattedLabel));
                }
            }
        }
        public string FormattedLabel => IsSelected ? $"> {Label}" : Label;

        public MenuItemViewModel(string label)
        {
            Label = label;
        }
    }

}
