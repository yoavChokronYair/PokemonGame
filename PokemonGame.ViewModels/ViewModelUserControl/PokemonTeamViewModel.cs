using System.Collections.ObjectModel;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelUserControl
{
    public class PokemonTeamViewModel : ViewModelBase
    {
        // Always exactly 6 slots — empty slots show placeholder
        public ObservableCollection<TeamSlotDisplayEntry> Slots { get; } = new();

        public PokemonTeamViewModel()
        {
            // Fill all 6 with empty placeholders by default
            for (int i = 0; i < 6; i++)
            {
                Slots.Add(TeamSlotDisplayEntry.Empty());
            }
        }

        /// <summary>
        /// Load from any list of entries — pads to 6 with empty slots.
        /// </summary>
        public void LoadSlots(IEnumerable<TeamSlotDisplayEntry> entries)
        {
            Slots.Clear();
            foreach (var e in entries.Take(6))
            {
                Slots.Add(e);
            }
            // Pad remaining with empty
            while (Slots.Count < 6)
            {
                Slots.Add(TeamSlotDisplayEntry.Empty());
            }
        }
    }
    public class TeamSlotDisplayEntry : ViewModelBase
    {
        private int _pokedexId;
        private bool _isEmpty;
        private string _heldItemName;

        public string HeldItemName { get => _heldItemName; set => SetProperty(ref _heldItemName, value); }
        public int PokedexId { get => _pokedexId; set => SetProperty(ref _pokedexId, value); }
        public bool IsEmpty { get => _isEmpty; set => SetProperty(ref _isEmpty, value); }

        public static TeamSlotDisplayEntry Empty() => new() { IsEmpty = true };
    }
}