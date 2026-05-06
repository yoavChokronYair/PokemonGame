using PokemonGame.ViewModels.ViewModelPage.BattleMenu;

namespace PokemonGame.ViewModels.ViewModelHelper.Service
{
    public enum BattleResultAction
    {
        NewGame,
        Back,
        Rematch
    }
    public interface IDialogService
    {
        Task<bool> ShowConfirmAsync(string title, string message);
        Task ShowMessageAsync(string message);
        Task ShowErrorAsync(string title, string message);
        Task ShowSuccessAsync(string title, string message);
        Task<string> ShowInputAsync(string title, string message, string defaultValue = "");
        Task<string> ShowSelectionAsync(string title, string message, IEnumerable<string> options);
        Task<BattleResultAction> ShowBattleResultAsync(BattleViewModel vm);
    }
}