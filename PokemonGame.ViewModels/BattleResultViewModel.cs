using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels
{
    public class BattleResultViewModel : ViewModelBase
    {
        // ── Winner & method ──────────────────────────────────────────────
        private string _winnerText = "BLACK WON";
        public string WinnerText
        {
            get => _winnerText;
            set => SetProperty(ref _winnerText, value);
        }

        private string _resultMethod = "by resignation";
        public string ResultMethod
        {
            get => _resultMethod;
            set => SetProperty(ref _resultMethod, value);
        }

        // ── Rank section ─────────────────────────────────────────────────
        private string _rankName = "Gold III";
        public string RankName
        {
            get => _rankName;
            set => SetProperty(ref _rankName, value);
        }

        private int _rankDelta = -25;
        public int RankDelta
        {
            get => _rankDelta;
            set
            {
                if (SetProperty(ref _rankDelta, value))
                {
                    OnPropertyChanged(nameof(RankDeltaText));
                    OnPropertyChanged(nameof(IsPositiveDelta));
                }
            }
        }

        /// <summary>Formatted string: "+18" or "-25".</summary>
        public string RankDeltaText => RankDelta >= 0 ? $"+{RankDelta}" : $"{RankDelta}";

        /// <summary>True → green animation, False → red animation. Drives DataTriggers.</summary>
        public bool IsPositiveDelta => RankDelta >= 0;

        // ── Progress bar ─────────────────────────────────────────────────
        private int _ratingCurrent = 35;
        public int RatingCurrent
        {
            get => _ratingCurrent;
            set
            {
                if (SetProperty(ref _ratingCurrent, value))
                    OnPropertyChanged(nameof(RatingText));
            }
        }

        private int _ratingMax = 100;
        public int RatingMax
        {
            get => _ratingMax;
            set
            {
                if (SetProperty(ref _ratingMax, value))
                    OnPropertyChanged(nameof(RatingText));
            }
        }

        public string RatingText => $"{RatingCurrent}/{RatingMax}";

        // ── Commands ─────────────────────────────────────────────────────
        public ICommand NewGameCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand RematchCommand { get; }

        public event EventHandler<BattleResultAction>? CloseRequested;

        public BattleResultViewModel()
        {
            NewGameCommand = new RelayCommand(() => CloseRequested?.Invoke(this, BattleResultAction.NewGame));
            BackCommand = new RelayCommand(() => CloseRequested?.Invoke(this, BattleResultAction.Back));
            RematchCommand = new RelayCommand(() => CloseRequested?.Invoke(this, BattleResultAction.Rematch));
        }

        // ── Factory helper ───────────────────────────────────────────────
        public static BattleResultAction ShowDialog(
            string winnerText,
            string resultMethod,
            string rankName,
            int rankDelta,
            int ratingCurrent,
            int ratingMax,
            Window? owner = null)
        {
            var vm = new BattleResultViewModel
            {
                WinnerText = winnerText,
                ResultMethod = resultMethod,
                RankName = rankName,
                RankDelta = rankDelta,
                RatingCurrent = ratingCurrent,
                RatingMax = ratingMax,
            };

            var window = new PokemonGame.Views.Windows.BattleResult { DataContext = vm };
            if (owner != null) window.Owner = owner;

            BattleResultAction chosen = BattleResultAction.Back;
            vm.CloseRequested += (_, action) =>
            {
                chosen = action;
                window.Close();
            };

            window.ShowDialog();
            return chosen;
        }
    }

    public enum BattleResultAction { NewGame, Back, Rematch }
}