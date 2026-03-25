using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;

namespace PokemonGame.ViewModels.ViewModelUserControl
{
    // TeamBuilderState.cs
    public class TeamBuilderState : ViewModelBase
    {
        private TeamSlotEntry _selectedPokemon;
        public TeamSlotEntry SelectedPokemon
        {
            get => _selectedPokemon;
            set => SetProperty(ref _selectedPokemon, value);
        }

        private int _selectedSlotIndex = -1;
        public int SelectedSlotIndex
        {
            get => _selectedSlotIndex;
            set => SetProperty(ref _selectedSlotIndex, value);
        }

        private bool _isEvIvPanelOpen;
        public bool IsEvIvPanelOpen
        {
            get => _isEvIvPanelOpen;
            set => SetProperty(ref _isEvIvPanelOpen, value);
        }

        private bool _isMovePickerOpen;
        public bool IsMovePickerOpen
        {
            get => _isMovePickerOpen;
            set => SetProperty(ref _isMovePickerOpen, value);
        }

        private bool _isItemPickerOpen;
        public bool IsItemPickerOpen
        {
            get => _isItemPickerOpen;
            set => SetProperty(ref _isItemPickerOpen, value);
        }

        private bool _isPokemonPickerOpen;
        public bool IsPokemonPickerOpen
        {
            get => _isPokemonPickerOpen;
            set => SetProperty(ref _isPokemonPickerOpen, value);
        }

        private int _activeMoveSlot;
        public int ActiveMoveSlot
        {
            get => _activeMoveSlot;
            set => SetProperty(ref _activeMoveSlot, value);
        }

        public ObservableCollection<TeamSlotEntry> TeamSlots { get; }
            = new ObservableCollection<TeamSlotEntry>(new TeamSlotEntry[6]);
        public TeamBuilderState()
        {
            CloseAllPanels();
            ActiveMoveSlot = 0;
        }
        public void CloseAllPanels()
        {
            IsEvIvPanelOpen = false;
            IsMovePickerOpen = false;
            IsItemPickerOpen = false;
            IsPokemonPickerOpen = false;
        }
    }
}
