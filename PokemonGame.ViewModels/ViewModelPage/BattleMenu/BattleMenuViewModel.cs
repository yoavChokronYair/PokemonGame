using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class BattleMenuViewModel : ViewModelBase
    {
        private bool _isMovesetVisible;

        public bool IsMovesetVisible
        {
            get => _isMovesetVisible;
            set
            {
                if (SetProperty(ref _isMovesetVisible, value))
                {
                    // When this changes, the MainMenu visibility must also update
                    OnPropertyChanged(nameof(IsMainMenuVisible));
                }
            }
        }

        // This is the "Inverse" property for your Converter
        public bool IsMainMenuVisible => !IsMovesetVisible;

        public ICommand OpenMovesetCommand { get; }
        public ICommand CloseMovesetCommand { get; }

        public BattleMenuViewModel()
        {
            IsMovesetVisible = false;
            OpenMovesetCommand = new RelayCommand(() => IsMovesetVisible = true);
            CloseMovesetCommand = new RelayCommand(() => IsMovesetVisible = false);
        }
    }
}
