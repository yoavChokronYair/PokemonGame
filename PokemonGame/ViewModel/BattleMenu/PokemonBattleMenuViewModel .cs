using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;

namespace PokemonGame.ViewModel.BattleMenu
{
    public class PokemonBattleMenuViewModel : ViewModelBase
    {
        private readonly NavigationStore _NavigationStore;
        public ViewModelBase CurrentViewModel => _NavigationStore.CurrentViewModel;
        public ICommand KeyPressedCommand { get; }
        public PokemonBattleMenuViewModel(NavigationStore navigationStore)
        {
           
            KeyPressedCommand = new RelayCommand<KeyEventArgs>(OnKeyPressed);
            
            UpdateMenuTexts();
            this._NavigationStore = navigationStore;
            _NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

        }
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
        private readonly string[,] baseMenuTexts = new string[,]
        {
            { "FIGHT", "BAG" },
            { "POKeMON", "RUN" }
        };

        private int selectedRow = 0;
        private int selectedCol = 0;

        private string _fightText;
        public string FightText
        {
            get => _fightText;
            private set
            {
                _fightText = value;
                OnPropertyChanged(nameof(FightText));
            }
        }

        private string _bagText;
        public string BagText
        {
            get => _bagText;
            private set
            {
                _bagText = value;
                OnPropertyChanged(nameof(BagText));
            }
        }

        private string _pokemonText;
        public string PokemonText
        {
            get => _pokemonText;
            private set
            {
                _pokemonText = value;
                OnPropertyChanged(nameof(PokemonText));
            }
        }

        private string _runText;
        public string RunText
        {
            get => _runText;
            private set
            {
                _runText = value;
                OnPropertyChanged(nameof(RunText));
            }
        }


        private void OnKeyPressed(KeyEventArgs e)
        {
            int maxRow = baseMenuTexts.GetLength(0) - 1;
            int maxCol = baseMenuTexts.GetLength(1) - 1;

            switch (e.Key)
            {
                case System.Windows.Input.Key.Up:
                    if (selectedRow > 0)
                        selectedRow--;
                    break;
                case System.Windows.Input.Key.Down:
                    if (selectedRow < maxRow)
                        selectedRow++;
                    break;
                case System.Windows.Input.Key.Left:
                    if (selectedCol > 0)
                        selectedCol--;
                    break;
                case System.Windows.Input.Key.Right:
                    if (selectedCol < maxCol)
                        selectedCol++;
                    break;
                case System.Windows.Input.Key.Enter:
                    _NavigationStore.CurrentViewModel = new MoveViewModel();
                    break;
            }
            UpdateMenuTexts();
        }

        private void UpdateMenuTexts()
        {
            FightText = (selectedRow == 0 && selectedCol == 0) ? $"> {baseMenuTexts[0, 0]}" : baseMenuTexts[0, 0];
            BagText = (selectedRow == 0 && selectedCol == 1) ? $"> {baseMenuTexts[0, 1]}" : baseMenuTexts[0, 1];
            PokemonText = (selectedRow == 1 && selectedCol == 0) ? $"> {baseMenuTexts[1, 0]}" : baseMenuTexts[1, 0];
            RunText = (selectedRow == 1 && selectedCol == 1) ? $"> {baseMenuTexts[1, 1]}" : baseMenuTexts[1, 1];

            // Reset all visibilities

        }
    }
}
