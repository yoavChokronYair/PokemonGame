using System.Windows.Input;

namespace PokemonGame.ViewModels.ViewModelPage.Map.Command
{
    public class PickChoiceCommand : ICommand
    {
        private readonly MapViewModel _map;
        private readonly int _index;

        public PickChoiceCommand(MapViewModel map, int index)
        {
            _map = map;
            _index = index;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
            => _map.Dialogue.IsOpen && _map.Dialogue.HasChoices
               && _index < _map.Dialogue.Choices.Count;

        public void Execute(object? parameter)
            => _map.Dialogue.PickChoice(_index);
    }
}