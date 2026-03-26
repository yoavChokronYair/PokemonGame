using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;

namespace PokemonGame.ViewModels.ViewModelUserControl
{
    public class TeamSlotBarViewModel : ViewModelBase
    {
        private readonly TeamBuilderState _state;

        public ObservableCollection<TeamSlotEntry> TeamSlots => _state.TeamSlots;

        public RelayCommand<TeamSlotEntry> SelectSlotCommand { get; }
        public RelayCommand RemoveFromTeamCommand { get; }

        public TeamSlotBarViewModel(TeamBuilderState state, TeamBuilderService service)
        {
            _state = state;

            SelectSlotCommand = new RelayCommand<TeamSlotEntry>(slot =>
            {
                if (slot == null)
                {
                    for (int i = 0; i < _state.TeamSlots.Count; i++)
                    {
                        if (_state.TeamSlots[i] == null) { _state.SelectedSlotIndex = i; break; }
                    }
                    _state.CloseAllPanels();
                    _state.SelectedPokemon = null;
                    _state.IsPokemonPickerOpen = true;
                }
                else
                {
                    _state.SelectedSlotIndex = _state.TeamSlots.IndexOf(slot);
                    _state.CloseAllPanels();
                    _state.SelectedPokemon = slot;
                    _state.ActiveMoveSlot = 0;
                }
            });

            RemoveFromTeamCommand = new RelayCommand(() =>
            {
                if (_state.SelectedPokemon == null)
                {
                    return;
                }

                int idx = _state.TeamSlots.IndexOf(_state.SelectedPokemon);
                if (idx >= 0)
                {
                    _state.TeamSlots[idx] = null;
                }

                _state.SelectedPokemon = null;
            });
        }
    }
}
