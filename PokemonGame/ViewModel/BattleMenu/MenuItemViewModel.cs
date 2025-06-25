using PokemonGame.ViewModel.ViewModelHelper;
namespace PokemonGame.ViewModel.BattleMenu
{
    public class MenuItemViewModel : ViewModelBase
    {
        public string Label { get; }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
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
