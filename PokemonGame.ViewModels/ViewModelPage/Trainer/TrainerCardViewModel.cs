using System.Collections.ObjectModel;
using PokemonGame.Model.Domain.Player;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Trainer
{
    public class TrainerCardViewModel : ViewModelBase
    {
        private string _trainerId;
        private string _trainerName;
        private string _playTime;
        private string _trainerCardImage;
        private string _money;

        public string TrainerId
        {
            get => _trainerId;
            set => SetProperty(ref _trainerId, value);
        }

        public string TrainerName
        {
            get => _trainerName;
            set => SetProperty(ref _trainerName, value);
        }

        public string PlayTime
        {
            get => _playTime;
            set => SetProperty(ref _playTime, value);
        }

        public string TrainerCardImage
        {
            get => _trainerCardImage;
            set => SetProperty(ref _trainerCardImage, value);
        }
        public string Money
        {
            get => _money;
            set => SetProperty(ref _money, value);
        }

        public ObservableCollection<BadgeDomain> Badges { get; set; }

        public TrainerCardViewModel()
        {
            TrainerId = "5555555";
            TrainerName = "Leaf";
            PlayTime = "12:45";
            Money = "1444";
            TrainerCardImage = "/Assets/Images/TrainerCard/TrainerCardLeafFront.png";

            PlayerDomain.Instance.Badges = new()
            {
                new BadgeDomain { Id = 1, IsObtained = false },
                new BadgeDomain { Id = 2, IsObtained = false },
                new BadgeDomain { Id = 3, IsObtained = false },
                new BadgeDomain { Id = 4, IsObtained = false },
                new BadgeDomain { Id = 5, IsObtained = false },
                new BadgeDomain { Id = 6, IsObtained = false },
                new BadgeDomain { Id = 7, IsObtained = false },
                new BadgeDomain { Id = 8, IsObtained = false }
            };
            Badges = new ObservableCollection<BadgeDomain>
            (
                PlayerDomain.Instance.Badges
            );
        }
    }
}