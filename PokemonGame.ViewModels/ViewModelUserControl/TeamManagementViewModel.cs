using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelPage.OnlineBattle;

namespace PokemonGame.ViewModels.ViewModelUserControl
{
    public class TeamManagementViewModel : ViewModelBase
    {
        private readonly TeamBuilderState _state;
        private readonly ITeamService _teamService;
        private readonly IPokedexService _pokedexService;
        private readonly UserStore _userStore;

        private readonly ObservableCollection<PokemonDisplayEntry> _allPokemon;
        private readonly ObservableCollection<ItemData> _allItems;


        private string _teamName = "My Team";
        public string TeamName
        {
            get => _teamName;
            set => SetProperty(ref _teamName, value);
        }

        private List<TeamData> _savedTeams = new List<TeamData>();
        public List<TeamData> SavedTeams
        {
            get => _savedTeams;
            set => SetProperty(ref _savedTeams, value);
        }

        private TeamData _selectedTeam;
        public TeamData SelectedTeam
        {
            get => _selectedTeam;
            set
            {
                if (SetProperty(ref _selectedTeam, value))
                {
                    if (value != null)
                    {
                        TeamName = value.TeamName ?? "My Team";
                    }

                    OnPropertyChanged(nameof(CanCreateNewTeam));
                }
            }
        }

        public bool CanCreateNewTeam => SavedTeams.Count < 3;

        public RelayCommand SaveTeamCommand { get; }
        public RelayCommand LoadTeamCommand { get; }
        public RelayCommand NewTeamCommand { get; }
        public RelayCommand<TeamData> DeleteTeamCommand { get; }

        public TeamManagementViewModel(
        TeamBuilderState state,
        ITeamService teamService,
        IPokedexService pokedexService,
        UserStore userStore,
        ObservableCollection<PokemonDisplayEntry> allPokemon,
        ObservableCollection<ItemData> allItems)
        {
            _state = state;
            _teamService = teamService;
            _pokedexService = pokedexService;
            _userStore = userStore;
            _allPokemon = allPokemon;
            _allItems = allItems;

            SaveTeamCommand = new RelayCommand(() =>
            {
                var battlerPokemons = BuildBattlerPokemons();
                if (battlerPokemons.Count == 0)
                {
                    return;
                }

                if (SelectedTeam != null)
                {
                    _teamService.UpdateTeam(SelectedTeam.Id, TeamName, battlerPokemons);
                }
                else
                {
                    if (!_teamService.CanCreateTeam(_userStore.BattlePlayerID))
                    {
                        return;
                    }

                    var team = _teamService.SaveTeam(TeamName, _userStore.BattlePlayerID,
                                                 battlerPokemons);
                    if (team != null)
                    {
                        SelectedTeam = team;
                    }
                }
                RefreshSavedTeams();
            });

            LoadTeamCommand = new RelayCommand(() =>
            {
                if (SelectedTeam == null)
                {
                    return;
                }

                TeamName = SelectedTeam.TeamName ?? "My Team";

                // Clear all slots
                for (int i = 0; i < 6; i++)
                {
                    _state.TeamSlots[i] = null;
                }

                _state.SelectedPokemon = null;

                // Load members from DB
                var members = _teamService.GetTeamMembers(SelectedTeam.Id);
                for (int i = 0; i < members.Count && i < 6; i++)
                {
                    var bp = members[i];
                    var pokemon = _allPokemon.FirstOrDefault(p => p.PokedexId == bp.PokedexID);
                    if (pokemon == null)
                    {
                        continue;
                    }

                    var slot = new TeamSlotEntry(pokemon)
                    {
                        Level = bp.Level,
                        Gender = bp.Gender switch
                        {
                            "Male" => "♂",
                            "Female" => "♀",
                            _ => "—"
                        },
                        IsShiny = bp.Shiny == 1,
                        Nature = bp.Nature ?? "Serious",
                        HeldItemName = _allItems.FirstOrDefault(it => it.Id == bp.ItemID)?.Name,
                        SelectedAbility = _pokedexService.GetAbilityNameById(bp.AbilityID),
                        IvHP = bp.Iv_hp,
                        IvAtk = bp.Iv_atk,
                        IvDef = bp.Iv_def,
                        IvSpA = bp.Iv_spAtk,
                        IvSpD = bp.Iv_spDef,
                        IvSpe = bp.Iv_speed,
                        EvHP = bp.Ev_hp,
                        EvAtk = bp.Ev_atk,
                        EvDef = bp.Ev_def,
                        EvSpA = bp.Ev_spAtk,
                        EvSpD = bp.Ev_spDef,
                        EvSpe = bp.Ev_speed,
                    };
                    slot.Move1 = _pokedexService.GetMoveById(bp.Move1ID, pokemon.AvailableMoves);
                    slot.Move2 = _pokedexService.GetMoveById(bp.Move2ID, pokemon.AvailableMoves);
                    slot.Move3 = _pokedexService.GetMoveById(bp.Move3ID, pokemon.AvailableMoves);
                    slot.Move4 = _pokedexService.GetMoveById(bp.Move4ID, pokemon.AvailableMoves);

                    _state.TeamSlots[i] = slot;
                }
                _state.CloseAllPanels();
            });

            NewTeamCommand = new RelayCommand(() =>
            {
                SelectedTeam = null;
                TeamName = "My Team";
                for (int i = 0; i < 6; i++)
                {
                    _state.TeamSlots[i] = null;
                }

                _state.SelectedPokemon = null;
            }, () => CanCreateNewTeam);

            DeleteTeamCommand = new RelayCommand<TeamData>(team =>
            {
                if (team == null)
                {
                    return;
                }

                _teamService.DeleteTeam(team.Id);
                if (SelectedTeam?.Id == team.Id)
                {
                    SelectedTeam = null;
                    TeamName = "My Team";
                    for (int i = 0; i < 6; i++)
                    {
                        _state.TeamSlots[i] = null;
                    }
                }
                RefreshSavedTeams();
            });

            RefreshSavedTeams();
        }

        private void RefreshSavedTeams()
        {
            SavedTeams = _teamService.GetTeamsByBattlePlayer(_userStore.BattlePlayerID);
            OnPropertyChanged(nameof(SavedTeams));
            OnPropertyChanged(nameof(CanCreateNewTeam));
        }

        private List<BattlerPokemon> BuildBattlerPokemons()
        {
            var list = new List<BattlerPokemon>();
            foreach (var slot in _state.TeamSlots)
            {
                if (slot == null)
                {
                    continue;
                }

                var move1 = slot.Move1 ?? slot.AvailableMoves.FirstOrDefault();
                if (move1 == null)
                {
                    continue;
                }

                list.Add(new BattlerPokemon
                {
                    PokedexID = slot.PokedexId,
                    AbilityID = _pokedexService.GetAbilityId(slot.SelectedAbility),
                    ItemID = _pokedexService.GetItemId(slot.HeldItemName),
                    Shiny = slot.IsShiny ? 1 : 0,
                    Gender = slot.Gender switch
                    {
                        "♂" => "Male",
                        "♀" => "Female",
                        "—" => new Random().Next(2) == 0 ? "Male" : "Female",
                        _ => "Genderless"
                    },
                    Level = slot.Level,
                    Move1ID = _pokedexService.GetMoveId(move1.Name) ?? 0,
                    Move2ID = _pokedexService.GetMoveId(slot.Move2?.Name),
                    Move3ID = _pokedexService.GetMoveId(slot.Move3?.Name),
                    Move4ID = _pokedexService.GetMoveId(slot.Move4?.Name),
                    Iv_hp = slot.IvHP,
                    Iv_atk = slot.IvAtk,
                    Iv_def = slot.IvDef,
                    Iv_spAtk = slot.IvSpA,
                    Iv_spDef = slot.IvSpD,
                    Iv_speed = slot.IvSpe,
                    Ev_hp = slot.EvHP,
                    Ev_atk = slot.EvAtk,
                    Ev_def = slot.EvDef,
                    Ev_spAtk = slot.EvSpA,
                    Ev_spDef = slot.EvSpD,
                    Ev_speed = slot.EvSpe,
                    Nature = slot.Nature ?? "Hardy",
                });
            }
            return list;
        }
    }
}
