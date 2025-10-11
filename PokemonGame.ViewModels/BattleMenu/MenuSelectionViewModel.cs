using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.BattleMenu
{
    public class MenuSelectionViewModel :ViewModelBase
    {
        public int Rows => 2;
        public int Columns => 2;

        private int selectedRow;
        public int SelectedRow
        {
            get => selectedRow;
            set => OnPropertyChanged(nameof(SelectedRow));
        }

        private int selectedCol;
        public int SelectedCol
        {
            get => selectedCol;
            set => OnPropertyChanged(nameof(SelectedCol));
        }
    }
}
