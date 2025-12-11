namespace PokemonGame.ViewModels.ViewModelHelper.Service
{
    public interface IDialogService
    {
        Task<bool> ShowConfirm(string title, string message);
        Task ShowMessage(string message);
        Task ShowError(string title, string message);
        Task ShowSuccess(string title, string message);
        Task<string> ShowInput(string title, string message, string defaultValue = "");
        Task<string> ShowSelection(string title, string message, IEnumerable<string> options);
    }
}