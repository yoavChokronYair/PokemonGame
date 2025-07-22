using CommunityToolkit.Mvvm.Input;
using PokemonGame.ViewModel.Map;
using PokemonGame.ViewModel.ViewModelHelper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PokemonGame.ViewModel.BattleMenu
{
    public class PokemonBattleMovesetMenuViewModel : ViewModelBase
    {
        private readonly NavigationStore _NavigationStore;
        public ICommand KeyPressedCommand { get; }
        public WildPokemonBattleViewModel wildPokemonBattleView { get; }
        public ObservableCollection<MoveViewModel> MoveList { get; }
        public MenuSelectionViewModel MenuSelection { get; }

        public ICommand DirectionCommand { get; }
        public ICommand ConfirmMoveCommand { get; }
        public ICommand CancelCommand { get; }
        public PokemonBattleMovesetMenuViewModel(NavigationStore navigationStore, WildPokemonBattleViewModel wildPokemonBattleView)
        {
            _NavigationStore = navigationStore;
            _NavigationStore.CurrentViewModel = this;
            this.wildPokemonBattleView = wildPokemonBattleView;

            MoveList = new ObservableCollection<MoveViewModel>(wildPokemonBattleView.MoveList);
            while (MoveList.Count < 4)
                MoveList.Add(new MoveViewModel { BaseName = "-", CurrentPP = 0 });

            MenuSelection = new MenuSelectionViewModel();
            UpdateSelectedMove();

            DirectionCommand = new RelayCommand<string>(OnDirectionInput);
            ConfirmMoveCommand = new AsyncRelayCommand(OnConfirmMove);
            CancelCommand = new RelayCommand(OnCancel);
            Move = MoveList[0];
        }
        private MoveViewModel move;
        public MoveViewModel Move
        {
            get => move;
            set
            {
                if (move != value)
                {
                    move = value;
                    OnPropertyChanged(nameof(Move));
                }
            }
        }

        private void UpdateSelectedMove()
        {
            for (int i = 0; i < MoveList.Count; i++)
            {
                int row = i / 2;
                int col = i % 2;
                MoveList[i].IsSelected = row == MenuSelection.SelectedRow && col == MenuSelection.SelectedCol;
                if (MoveList[i].IsSelected)
                {
                    Move = MoveList[i];
                }
            }
        }
        private void OnDirectionInput(string direction)
        {
            int row = MenuSelection.SelectedRow;
            int col = MenuSelection.SelectedCol;

            switch (direction)
            {
                case "Up": if (row > 0) row--; break;
                case "Down": if (row < 1) row++; break;
                case "Left": if (col > 0) col--; break;
                case "Right": if (col < 1) col++; break;
            }

            int index = row * 2 + col;
            if (index >= MoveList.Count || MoveList[index].BaseName == "-")
                return;

            MenuSelection.SelectedRow = row;
            MenuSelection.SelectedCol = col;
            
            UpdateSelectedMove();
        }

        private async Task OnConfirmMove()
        {
            int index = MenuSelection.SelectedRow * 2 + MenuSelection.SelectedCol;
            var selectedMove = MoveList[index];
            if (selectedMove.BaseName == "-") return;
            
            await wildPokemonBattleView.MakeMove(selectedMove.BaseName);
            if(wildPokemonBattleView._PageNavigationStore.CurrentViewModel != wildPokemonBattleView._mainWindow)
            {
                
                _NavigationStore.CurrentViewModel = (new PokemonBattleMenuViewModel(_NavigationStore, wildPokemonBattleView._PageNavigationStore, wildPokemonBattleView));
            }

        }

        private void OnCancel()
        {
            _NavigationStore.CurrentViewModel = (new PokemonBattleMenuViewModel(_NavigationStore, wildPokemonBattleView._PageNavigationStore, wildPokemonBattleView));
        }
    }
}
