using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class MenuSelectionViewModel : ViewModelBase
    {
        public int Rows => 2;
        public int Columns => 2;

        private int _selectedRow;
        public int SelectedRow
        {
            get => _selectedRow;
            set => OnPropertyChanged(nameof(SelectedRow));
        }

        private int _selectedCol;
        public int SelectedCol
        {
            get => _selectedCol;
            set => OnPropertyChanged(nameof(SelectedCol));
        }
    }
}
