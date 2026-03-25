using System.Collections.ObjectModel;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelUserControl;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class BattleHistoryEntry : ViewModelBase
    {
        public string PlayerName { get; set; } = string.Empty;
        public string OpponentName { get; set; } = string.Empty;
        public bool IsPlayerWinner { get; set; }
        public PokemonTeamViewModel PlayerTeam { get; } = new();
        public PokemonTeamViewModel OpponentTeam { get; } = new();
    }

    public class HistoryBattleViewModel : ViewModelBase
    {
        public ObservableCollection<BattleHistoryEntry> Battles { get; } = new();
        public bool HasNoBattles => Battles.Count == 0;

        public HistoryBattleViewModel(UserStore player)
        {
            LoadDummyBattles();
        }

        private void LoadDummyBattles()
        {
            var entry1 = new BattleHistoryEntry
            {
                PlayerName = "yoav",
                OpponentName = "Ash",
                IsPlayerWinner = true,
            };
            entry1.PlayerTeam.LoadSlots(new[]
            {
                new TeamSlotDisplayEntry { PokedexId = 6,   Name = "Charizard",  Type1 = "Fire",   Type2 = "Flying", IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 9,   Name = "Blastoise",  Type1 = "Water",  Type2 = null,     IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 3,   Name = "Venusaur",   Type1 = "Grass",  Type2 = "Poison", IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 25,  Name = "Pikachu",    Type1 = "Electric", Type2 = null,   IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 131, Name = "Lapras",     Type1 = "Water",  Type2 = "Ice",    IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 143, Name = "Snorlax",    Type1 = "Normal", Type2 = null,     IsEmpty = false },
            });
            entry1.OpponentTeam.LoadSlots(new[]
            {
                new TeamSlotDisplayEntry { PokedexId = 149, Name = "Dragonite",  Type1 = "Dragon", Type2 = "Flying", IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 130, Name = "Gyarados",   Type1 = "Water",  Type2 = "Flying", IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 59,  Name = "Arcanine",   Type1 = "Fire",   Type2 = null,     IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 65,  Name = "Alakazam",   Type1 = "Psychic", Type2 = null,    IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 68,  Name = "Machamp",    Type1 = "Fighting", Type2 = null,   IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 94,  Name = "Gengar",     Type1 = "Ghost",  Type2 = "Poison", IsEmpty = false },
            });

            var entry2 = new BattleHistoryEntry
            {
                PlayerName = "yoav",
                OpponentName = "Misty",
                IsPlayerWinner = false,
            };
            entry2.PlayerTeam.LoadSlots(new[]
            {
                new TeamSlotDisplayEntry { PokedexId = 6,   Name = "Charizard",  Type1 = "Fire",   Type2 = "Flying", IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 112, Name = "Rhydon",     Type1 = "Ground", Type2 = "Rock",   IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 76,  Name = "Golem",      Type1 = "Rock",   Type2 = "Ground", IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 38,  Name = "Ninetales",  Type1 = "Fire",   Type2 = null,     IsEmpty = false },
            });
            entry2.OpponentTeam.LoadSlots(new[]
            {
                new TeamSlotDisplayEntry { PokedexId = 121, Name = "Starmie",    Type1 = "Water",  Type2 = "Psychic", IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 117, Name = "Seadra",     Type1 = "Water",  Type2 = null,     IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 54,  Name = "Psyduck",    Type1 = "Water",  Type2 = null,     IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 90,  Name = "Shellder",   Type1 = "Water",  Type2 = null,     IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 98,  Name = "Krabby",     Type1 = "Water",  Type2 = null,     IsEmpty = false },
                new TeamSlotDisplayEntry { PokedexId = 116, Name = "Horsea",     Type1 = "Water",  Type2 = null,     IsEmpty = false },
            });

            Battles.Add(entry1);
            Battles.Add(entry2);

            OnPropertyChanged(nameof(HasNoBattles));
        }
    }
}