namespace PokemonGame.ViewModels.SignUp
{
    public interface IDialogService
    {
        Task<bool> ShowConfirm(string title, string message);
        Task ShowMessage(string message);
    }
}