using System;
using System.Collections.Generic;
using System.Text;
using PokemonGame.Model.Enums;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Map.Command
{
    public class MoveCommand : CommandBase
    {
        private readonly MapViewModel _vm;
        private readonly FacingDirection _direction;

        public MoveCommand(MapViewModel vm, FacingDirection direction)
        {
            _vm = vm;
            _direction = direction;
        }

        public override void Execute(object? parameter) => _vm.Move(_direction);
    }
}
