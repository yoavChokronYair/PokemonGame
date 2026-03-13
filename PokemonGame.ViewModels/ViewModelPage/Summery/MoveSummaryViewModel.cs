using System.Collections.ObjectModel;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Summery
{
    public class MoveSummaryViewModel : ViewModelBase
    {
        private readonly MoveService _moveService;
        private MoveSlotViewModel _selectedMove;

        public ObservableCollection<MoveSlotViewModel> KnownMoves { get; set; }

        public MoveSlotViewModel SelectedMove
        {
            get => _selectedMove;
            set { _selectedMove = value; OnPropertyChanged(nameof(SelectedMove)); }
        }

        public MoveSummaryViewModel()
        {
            KnownMoves = new ObservableCollection<MoveSlotViewModel>();
            _moveService = new MoveService();
            // For testing: Load a set of moves
            LoadMoves(new[] { "Flamethrower", "Thunderbolt", "Agility", "Toxic" });
        }

        private void LoadMoves(string[] names)
        {
            foreach (var name in names)
            {
                var tree = _moveService.GetMove(name);
                if (tree != null)
                {
                    KnownMoves.Add(new MoveSlotViewModel
                    {
                        Tree = tree,
                        CurrentPp = tree.Move.PP
                    });
                }
            }
            SelectedMove = KnownMoves.FirstOrDefault();
        }
    }


    public class MoveSlotViewModel : ViewModelBase
    {
        // The fully assembled tree from your MoveService
        public MoveTree Tree { get; set; }

        public string Name => Tree?.Move?.Name ?? "---";
        public string Type => Tree?.Move?.Element ?? "NORMAL";
        public string Category => Tree?.Move?.Category ?? "Status";
        public string Description => Tree?.Description ?? "";

        // Flattening logic: Find the first Power value in the tree
        public string Power
        {
            get
            {
                // Look for a 'FormulaDamage' or 'DirectDamage' effect in any attempt
                var damageEffect = Tree?.Attempts
                    .Select(a => a.OnHit)
                    .FirstOrDefault(e => e?.Type == "FormulaDamage" || e?.Type == "DirectDamage");

                if (damageEffect?.Number?.ExactValue != null)
                    return damageEffect.Number.ExactValue.ToString();

                return "---";
            }
        }

        public string Accuracy
        {
            get
            {
                var acc = Tree?.Attempts.FirstOrDefault()?.AccuracyValue;
                return acc.HasValue ? (acc.Value * 100).ToString() : "---";
            }
        }

        // PP Logic
        private int _currentPp;
        public int CurrentPp
        {
            get => _currentPp;
            set { _currentPp = value; OnPropertyChanged(nameof(CurrentPp)); OnPropertyChanged(nameof(PpColor)); }
        }
        public int MaxPp => Tree?.Move?.PP ?? 0;

        public string PpColor => (MaxPp > 0 && (double)CurrentPp / MaxPp <= 0.2) ? "#FF4500" : "#FFFFFF";
    }
}