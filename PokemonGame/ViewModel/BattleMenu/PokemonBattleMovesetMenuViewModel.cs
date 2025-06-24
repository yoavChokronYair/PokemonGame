using CommunityToolkit.Mvvm.Input;
using PokemonGame.ViewModel.ViewModelHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PokemonGame.ViewModel.BattleMenu
{
    public class PokemonBattleMovesetMenuViewModel : ViewModelBase
    {
        private readonly NavigationStore _NavigationStore;
        public ViewModelBase CurrentViewModel => _NavigationStore.CurrentViewModel;
        public ICommand KeyPressedCommand { get; }
        public WildPokemonBattleViewModel wildPokemonBattleView { get; }
        public PokemonBattleMovesetMenuViewModel(NavigationStore navigationStore, WildPokemonBattleViewModel wildPokemonBattleView)
        {
            this.wildPokemonBattleView = wildPokemonBattleView;
            foreach (var move in wildPokemonBattleView.MoveList)
            {
                move.Name = string.Join(" ", move.Name.Replace(">", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }
            this.MoveList = wildPokemonBattleView.MoveList;

            KeyPressedCommand = new RelayCommand<KeyEventArgs>(OnKeyPressed);
            var names = MoveList.Select(x => x.Name).ToList();
            while (names.Count < 4)
            {
                names.Add("-");
            }
            baseMenuTexts = new string[,]
            {
                { names[0], names[1] },
                { names[2], names[3] }
            };
            originalMoveNames = MoveList.Select(x => x.Name).ToList();
            UpdateMenuTexts();
            this._NavigationStore = navigationStore;

        }

        private readonly string[,] baseMenuTexts;
        private readonly List<string> originalMoveNames;


        private int selectedRow = 0;
        private int selectedCol = 0;

        private ObservableCollection<MoveViewModel> moveList;
        public ObservableCollection<MoveViewModel> MoveList
        {
            get => moveList;
            set
            {
                if (moveList != value)
                {
                    moveList = value;
                    OnPropertyChanged(nameof(MoveList));
                }
            }
        }


        private void OnKeyPressed(KeyEventArgs e)
        {

            int newRow = selectedRow;
            int newCol = selectedCol;

            int targetIndex = newRow * (1 + 1) + newCol;
            

            switch (e.Key)
            {
                case System.Windows.Input.Key.Up:
                    if (selectedRow == 1)
                        newRow = 0;
                    break;
                case System.Windows.Input.Key.Down:
                    if (selectedRow == 0)
                        newRow = 1;
                    break;
                case System.Windows.Input.Key.Left:
                    if (selectedCol == 1)
                        newCol = 0;
                    break;
                case System.Windows.Input.Key.Right:
                    if (selectedCol == 0)
                        newCol = 1;
                    break;
                case System.Windows.Input.Key.Escape:
                    _NavigationStore.CurrentViewModel = new PokemonBattleMenuViewModel(_NavigationStore, wildPokemonBattleView);
                    return;
                case System.Windows.Input.Key.Enter:    
                    if (baseMenuTexts[newRow, newCol] == "-")
                    {
                        selectedRow = newRow;
                        selectedCol = newCol;
                        return;
                    }
                     wildPokemonBattleView.MakeMove(baseMenuTexts[newRow,newCol]);
                    foreach (var move in wildPokemonBattleView.MoveList)
                    {
                        move.Name = string.Join(" ", move.Name.Replace(">", "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    }
                    _NavigationStore.CurrentViewModel = new PokemonBattleMenuViewModel(_NavigationStore, wildPokemonBattleView);

                    
                    break;

            }
            if (baseMenuTexts[newRow, newCol] == "-")
            {
                return;
            }
            selectedRow = newRow;
            selectedCol = newCol;
            // Prevent movement if the target move is "-"
            UpdateMenuTexts();
        }
    
     

        private void UpdateMenuTexts()
        {
            bool validSelection = selectedRow >= 0 && selectedCol >= 0 &&
                      selectedRow < baseMenuTexts.GetLength(0) &&
                      selectedCol < baseMenuTexts.GetLength(1);

            // Only update if a valid selection exists
            if (validSelection)
            {
                for (int row = 0; row < 2; row++)
                {
                    for (int col = 0; col < 2; col++)
                    {
                        int index = row * 2 + col;

                        if (index < MoveList.Count)
                        {
                            string baseName = baseMenuTexts[row, col];
                            MoveList[index].Name = (selectedRow == row && selectedCol == col)
                                ? $"> {baseName}"
                                : baseName;
                        }
                    }
                }
            }
        }
    }
}
