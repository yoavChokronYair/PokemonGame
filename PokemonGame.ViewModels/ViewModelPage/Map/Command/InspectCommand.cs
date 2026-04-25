using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Map.Command
{
    public class InspectCommand : CommandBase
    {
        private readonly MapViewModel _vm;

        public InspectCommand(MapViewModel vm)
        {
            _vm = vm;
        }

        public override void Execute(object? parameter) => _vm.Inspect();
    }
}
