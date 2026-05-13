using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Trainer
{
    public class TrainerCardViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        public string TrainerId { get; } = PlayerDomain.Instance.trainerInfo.TrainerID.ToString();
        public string TrainerName { get; } = PlayerDomain.Instance.trainerInfo.Name;
        public string PlayTime { get; } = PlayerDomain.Instance.trainerInfo.TimePlayed.ToString(@"hh\:mm\:ss");
        public string Money { get; } = PlayerDomain.Instance.trainerInfo.Money.ToString();
        public int HallOfFameDebut { get; } = PlayerDomain.Instance.trainerInfo.HallOfFameDebut;
        public ObservableCollection<PokemonPlayerDomain> Team { get; }
        


        public ObservableCollection<BadgeDomain> Badges { get; }
        public ICommand FlipCommand { get; }
        public ICommand ExistCommand { get; }

        private bool _isFront = true;
        private string _trainerCardImage;
        public string TrainerCardImage
        {
            get => _trainerCardImage;
            private set => SetProperty(ref _trainerCardImage, value);
        }

        public bool IsFront
        {
            get => _isFront;
            private set
            {
                SetProperty(ref _isFront, value);
                TrainerCardImage = ResolveCardImage(_isFront, PlayerDomain.Instance.trainerInfo.Gender);
            }
        }

        public TrainerCardViewModel(NavigationStore navigationStore,Func<ViewModelBase> MapViewModel)
        {
            _navigationStore = navigationStore; 
            TrainerCardImage = ResolveCardImage(_isFront, PlayerDomain.Instance.trainerInfo.Gender);
            Badges = InitBadges();
            Team = new ObservableCollection<PokemonPlayerDomain>(
                PlayerDomain.Instance.Team?.Members ?? Enumerable.Empty<PokemonPlayerDomain>()); 
            FlipCommand = new RelayCommand(() => IsFront = !IsFront);
            ExistCommand = new RelayCommand(() => _navigationStore.CurrentViewModel = MapViewModel());
        }

        private static string ResolveCardImage(bool isFront, Gender gender)
        {
            string side = isFront ? "Front" : "Back";
            string name = gender == Gender.Male ? "Red" : "Leaf";
            return $"/Assets/Images/TrainerCard/TrainerCard{name}{side}.png";
        }

        private static ObservableCollection<BadgeDomain> InitBadges()
        {
            
            return new ObservableCollection<BadgeDomain>(PlayerDomain.Instance.Badges);
        }
    }
}