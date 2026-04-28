using System.Windows.Input;
using PokemonGame.ViewModels.ViewModelPage.Dialogue;

namespace PokemonGame.ViewModels.ViewModelPage.Map.Command
{
    public class AdvanceDialogueCommand : ICommand
    {
        private readonly DialogueViewModel _dialogue;

        public AdvanceDialogueCommand(DialogueViewModel dialogue)
            => _dialogue = dialogue;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _dialogue.IsOpen;

        public void Execute(object? parameter)
        {
            if (parameter is DialogueChoiceViewModel choice)
                _dialogue.PickChoice(choice is DialogueChoiceViewModel c
                    ? _dialogue.Choices.IndexOf(c)
                    : 0);
            else
                _dialogue.Advance();
        }
    }
}