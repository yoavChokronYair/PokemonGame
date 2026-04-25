using System;
using System.Collections.Generic;
using System.Text;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Map.Command
{
    public class ShowLayerCommand : CommandBase
    {
        private readonly MapViewModel _vm;
        private readonly bool _background;

        public ShowLayerCommand(MapViewModel vm, bool background)
        {
            _vm = vm;
            _background = background;
        }

        public override void Execute(object? parameter) => _vm.SwitchLayer(_background);
    }
}
