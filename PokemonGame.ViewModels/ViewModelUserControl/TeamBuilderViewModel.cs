using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class TeamBuilderViewModel : ViewModelBase
    {
        // ── Service ───────────────────────────────────────────────────────────
        private readonly TeamBuilderService _service = new TeamBuilderService();

        // ── Static option lists ───────────────────────────────────────────────
        public List<string> NatureOptions { get; } = new List<string>
        {
            "Hardy",  "Lonely", "Brave",   "Adamant", "Naughty",
            "Bold",   "Docile", "Relaxed", "Impish",  "Lax",
            "Timid",  "Hasty",  "Serious", "Jolly",   "Naive",
            "Modest", "Mild",   "Quiet",   "Bashful", "Rash",
            "Calm",   "Gentle", "Sassy",   "Careful", "Quirky"
        };

        public List<string> GenderOptions { get; } = new List<string> { "—", "♂", "♀" };

        // ── All available pokemon ─────────────────────────────────────────────
        // PokemonDisplayEntry is the DTO defined in TeamBuilderService
        public ObservableCollection<PokemonDisplayEntry> AllPokemon { get; }

        // ── All held items ────────────────────────────────────────────────────
        // ItemData is PokemonGame.Services.Data.GameData.ItemData
        public ObservableCollection<ItemData> AllItems { get; }

        // ── Team slots ────────────────────────────────────────────────────────
        // Each slot is a BattlerPokemon extended with display fields via TeamSlotEntry
        public ObservableCollection<TeamSlotEntry> TeamSlots { get; }
            = new ObservableCollection<TeamSlotEntry>();

        // ── Selected slot in team ─────────────────────────────────────────────
        private TeamSlotEntry _selectedPokemon;
        public TeamSlotEntry SelectedPokemon
        {
            get => _selectedPokemon;
            set
            {
                if (SetProperty(ref _selectedPokemon, value))
                {
                    IsEvIvPanelOpen = false;
                    IsMovePickerOpen = false;
                    IsItemPickerOpen = false;
                    IsPokemonPickerOpen = false;
                    ActiveMoveSlot = 0;
                }
            }
        }

        // ── Selected item in picker — auto-confirms on set ────────────────────
        private ItemData _selectedItem;
        public ItemData SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
                    ConfirmItemCommand.Execute(ConfirmItemCommand);
            }
        }

        // ── Selected pokemon in picker — auto-confirms on set ─────────────────
        private PokemonDisplayEntry _pickerPokemon;
        public PokemonDisplayEntry PickerPokemon
        {
            get => _pickerPokemon;
            set
            {
                if (SetProperty(ref _pickerPokemon, value) && value != null)
                    ConfirmPokemonCommand.Execute(ConfirmPokemonCommand);
            }
        }

        // ── Selected move in picker — auto-confirms on set ────────────────────
        private string _selectedMove;
        public string SelectedMove
        {
            get => _selectedMove;
            set
            {
                if (SetProperty(ref _selectedMove, value) && value != null)
                    ConfirmMoveCommand.Execute(ConfirmMoveCommand);
            }
        }

        // ── Panel flags ───────────────────────────────────────────────────────
        private bool _isEvIvPanelOpen;
        public bool IsEvIvPanelOpen
        {
            get => _isEvIvPanelOpen;
            set => SetProperty(ref _isEvIvPanelOpen, value);
        }

        private bool _isMovePickerOpen;
        public bool IsMovePickerOpen
        {
            get => _isMovePickerOpen;
            set => SetProperty(ref _isMovePickerOpen, value);
        }

        private bool _isItemPickerOpen;
        public bool IsItemPickerOpen
        {
            get => _isItemPickerOpen;
            set => SetProperty(ref _isItemPickerOpen, value);
        }

        private bool _isPokemonPickerOpen;
        public bool IsPokemonPickerOpen
        {
            get => _isPokemonPickerOpen;
            set => SetProperty(ref _isPokemonPickerOpen, value);
        }

        private int _activeMoveSlot;
        public int ActiveMoveSlot
        {
            get => _activeMoveSlot;
            set => SetProperty(ref _activeMoveSlot, value);
        }

        // ── Commands ──────────────────────────────────────────────────────────
        public RelayCommand ToggleEvIvCommand { get; }
        public RelayCommand OpenMoveSlot1Command { get; }
        public RelayCommand OpenMoveSlot2Command { get; }
        public RelayCommand OpenMoveSlot3Command { get; }
        public RelayCommand OpenMoveSlot4Command { get; }
        public RelayCommand ConfirmMoveCommand { get; }
        public RelayCommand ConfirmItemCommand { get; }
        public RelayCommand ConfirmPokemonCommand { get; }
        public RelayCommand OpenItemPickerCommand { get; }
        public RelayCommand OpenPokemonPickerCommand { get; }
        public RelayCommand AddTeamSlotCommand { get; }
        public RelayCommand RemoveFromTeamCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────
        public TeamBuilderViewModel()
        {
            AllPokemon = new ObservableCollection<PokemonDisplayEntry>(
                _service.GetAllPokemon());

            AllItems = new ObservableCollection<ItemData>(
                _service.GetHeldItems());

            ToggleEvIvCommand = new RelayCommand(() =>
            {
                IsEvIvPanelOpen = !IsEvIvPanelOpen;
                if (IsEvIvPanelOpen)
                {
                    IsMovePickerOpen = false;
                    IsItemPickerOpen = false;
                    IsPokemonPickerOpen = false;
                }
            });

            OpenMoveSlot1Command = new RelayCommand(() => OpenMoveSlot(1));
            OpenMoveSlot2Command = new RelayCommand(() => OpenMoveSlot(2));
            OpenMoveSlot3Command = new RelayCommand(() => OpenMoveSlot(3));
            OpenMoveSlot4Command = new RelayCommand(() => OpenMoveSlot(4));

            ConfirmMoveCommand = new RelayCommand(() =>
            {
                if (SelectedPokemon == null || SelectedMove == null) return;
                switch (ActiveMoveSlot)
                {
                    case 1: SelectedPokemon.Move1 = SelectedMove; break;
                    case 2: SelectedPokemon.Move2 = SelectedMove; break;
                    case 3: SelectedPokemon.Move3 = SelectedMove; break;
                    case 4: SelectedPokemon.Move4 = SelectedMove; break;
                }
                SelectedMove = null;
                IsMovePickerOpen = false;
            });

            OpenItemPickerCommand = new RelayCommand(() =>
            {
                IsItemPickerOpen = !IsItemPickerOpen;
                if (IsItemPickerOpen)
                {
                    IsEvIvPanelOpen = false;
                    IsMovePickerOpen = false;
                    IsPokemonPickerOpen = false;
                }
            });

            ConfirmItemCommand = new RelayCommand(() =>
            {
                if (SelectedPokemon == null || SelectedItem == null) return;
                SelectedPokemon.HeldItemName = SelectedItem.Name;
                SelectedItem = null;
                IsItemPickerOpen = false;
            });

            OpenPokemonPickerCommand = new RelayCommand(() =>
            {
                IsPokemonPickerOpen = !IsPokemonPickerOpen;
                if (IsPokemonPickerOpen)
                {
                    IsEvIvPanelOpen = false;
                    IsMovePickerOpen = false;
                    IsItemPickerOpen = false;
                }
            });

            ConfirmPokemonCommand = new RelayCommand(() =>
            {
                if (PickerPokemon == null) return;
                var slot = CreateSlot(PickerPokemon);
                if (SelectedPokemon != null && TeamSlots.Contains(SelectedPokemon))
                {
                    int idx = TeamSlots.IndexOf(SelectedPokemon);
                    TeamSlots[idx] = slot;
                }
                else if (TeamSlots.Count < 6)
                {
                    TeamSlots.Add(slot);
                }
                PickerPokemon = null;
                IsPokemonPickerOpen = false;
                SelectedPokemon = slot;
            });

            AddTeamSlotCommand = new RelayCommand(() =>
            {
                if (TeamSlots.Count >= 6) return;
                IsPokemonPickerOpen = true;
            });

            RemoveFromTeamCommand = new RelayCommand(() =>
            {
                if (SelectedPokemon == null || !TeamSlots.Contains(SelectedPokemon)) return;
                TeamSlots.Remove(SelectedPokemon);
                SelectedPokemon = TeamSlots.Count > 0 ? TeamSlots[0] : null;
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void OpenMoveSlot(int slot)
        {
            ActiveMoveSlot = slot;
            IsMovePickerOpen = true;
            IsEvIvPanelOpen = false;
            IsItemPickerOpen = false;
            IsPokemonPickerOpen = false;
            SelectedMove = null;
        }

        /// <summary>
        /// Creates a fresh editable TeamSlotEntry from a picker template.
        /// </summary>
        private static TeamSlotEntry CreateSlot(PokemonDisplayEntry src) =>
            new TeamSlotEntry(src);
    }

    // ── TeamSlotEntry ─────────────────────────────────────────────────────────
    // Wraps PokemonDisplayEntry (read-only template data) with all the observable
    // editable fields the editor card binds to.
    // Lives here because it is purely a ViewModel concern — no DB mapping needed.

    public class TeamSlotEntry : ViewModelBase
    {
        // ── Template data (read-only, from DB) ────────────────────────────────
        public int PokedexId { get; }
        public string Name { get; }
        public string Type1 { get; }
        public string? Type2 { get; }
        public List<string> Abilities { get; }
        public List<string> AvailableMoves { get; }
        public int HP { get; }
        public int Atk { get; }
        public int Def { get; }
        public int SpA { get; }
        public int SpD { get; }
        public int Spe { get; }
        public int BST => HP + Atk + Def + SpA + SpD + Spe;

        public TeamSlotEntry(PokemonDisplayEntry src)
        {
            PokedexId = src.PokedexId;
            Name = src.Name;
            Type1 = src.Type1;
            Type2 = src.Type2;
            Abilities = src.Abilities;
            AvailableMoves = src.AvailableMoves;
            HP = src.HP; Atk = src.Atk; Def = src.Def;
            SpA = src.SpA; SpD = src.SpD; Spe = src.Spe;

            // Defaults
            Nickname = src.Name;
            Level = 100;
            Nature = "Serious";
            Gender = "—";
            SelectedAbility = src.Abilities.Count > 0 ? src.Abilities[0] : null;
            IvHP = 31; IvAtk = 31; IvDef = 31;
            IvSpA = 31; IvSpD = 31; IvSpe = 31;
        }

        // ── Editable fields ───────────────────────────────────────────────────
        private string _nickname;
        public string Nickname
        {
            get => _nickname;
            set => SetProperty(ref _nickname, value);
        }

        private int _level;
        public int Level
        {
            get => _level;
            set
            {
                if (SetProperty(ref _level, value))
                {
                    OnPropertyChanged(nameof(FinalHP));
                    OnPropertyChanged(nameof(FinalAtk));
                    OnPropertyChanged(nameof(FinalDef));
                    OnPropertyChanged(nameof(FinalSpA));
                    OnPropertyChanged(nameof(FinalSpD));
                    OnPropertyChanged(nameof(FinalSpe));
                }
            }
        }

        private string _gender;
        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        private bool _isShiny;
        public bool IsShiny
        {
            get => _isShiny;
            set => SetProperty(ref _isShiny, value);
        }

        private string _nature;
        public string Nature
        {
            get => _nature;
            set
            {
                if (SetProperty(ref _nature, value))
                {
                    OnPropertyChanged(nameof(FinalAtk));
                    OnPropertyChanged(nameof(FinalDef));
                    OnPropertyChanged(nameof(FinalSpA));
                    OnPropertyChanged(nameof(FinalSpD));
                    OnPropertyChanged(nameof(FinalSpe));
                }
            }
        }

        private string _heldItemName;
        public string HeldItemName
        {
            get => _heldItemName;
            set => SetProperty(ref _heldItemName, value);
        }

        private string _selectedAbility;
        public string SelectedAbility
        {
            get => _selectedAbility;
            set => SetProperty(ref _selectedAbility, value);
        }

        // Moves
        private string _move1; public string Move1 { get => _move1; set => SetProperty(ref _move1, value); }
        private string _move2; public string Move2 { get => _move2; set => SetProperty(ref _move2, value); }
        private string _move3; public string Move3 { get => _move3; set => SetProperty(ref _move3, value); }
        private string _move4; public string Move4 { get => _move4; set => SetProperty(ref _move4, value); }

        // EVs
        private int _evHP;
        public int EvHP { get => _evHP; set { if (SetProperty(ref _evHP, value)) { OnPropertyChanged(nameof(FinalHP)); OnPropertyChanged(nameof(RemainingEvs)); } } }
        private int _evAtk;
        public int EvAtk { get => _evAtk; set { if (SetProperty(ref _evAtk, value)) { OnPropertyChanged(nameof(FinalAtk)); OnPropertyChanged(nameof(RemainingEvs)); } } }
        private int _evDef;
        public int EvDef { get => _evDef; set { if (SetProperty(ref _evDef, value)) { OnPropertyChanged(nameof(FinalDef)); OnPropertyChanged(nameof(RemainingEvs)); } } }
        private int _evSpA;
        public int EvSpA { get => _evSpA; set { if (SetProperty(ref _evSpA, value)) { OnPropertyChanged(nameof(FinalSpA)); OnPropertyChanged(nameof(RemainingEvs)); } } }
        private int _evSpD;
        public int EvSpD { get => _evSpD; set { if (SetProperty(ref _evSpD, value)) { OnPropertyChanged(nameof(FinalSpD)); OnPropertyChanged(nameof(RemainingEvs)); } } }
        private int _evSpe;
        public int EvSpe { get => _evSpe; set { if (SetProperty(ref _evSpe, value)) { OnPropertyChanged(nameof(FinalSpe)); OnPropertyChanged(nameof(RemainingEvs)); } } }

        public int RemainingEvs => 510 - EvHP - EvAtk - EvDef - EvSpA - EvSpD - EvSpe;

        // IVs
        private int _ivHP = 31; public int IvHP { get => _ivHP; set { if (SetProperty(ref _ivHP, value)) OnPropertyChanged(nameof(FinalHP)); } }
        private int _ivAtk = 31; public int IvAtk { get => _ivAtk; set { if (SetProperty(ref _ivAtk, value)) OnPropertyChanged(nameof(FinalAtk)); } }
        private int _ivDef = 31; public int IvDef { get => _ivDef; set { if (SetProperty(ref _ivDef, value)) OnPropertyChanged(nameof(FinalDef)); } }
        private int _ivSpA = 31; public int IvSpA { get => _ivSpA; set { if (SetProperty(ref _ivSpA, value)) OnPropertyChanged(nameof(FinalSpA)); } }
        private int _ivSpD = 31; public int IvSpD { get => _ivSpD; set { if (SetProperty(ref _ivSpD, value)) OnPropertyChanged(nameof(FinalSpD)); } }
        private int _ivSpe = 31; public int IvSpe { get => _ivSpe; set { if (SetProperty(ref _ivSpe, value)) OnPropertyChanged(nameof(FinalSpe)); } }

        // ── Stat formula (Gen 3+) ─────────────────────────────────────────────
        private static readonly Dictionary<string, (string Boost, string Reduce)> NatureMap =
            new Dictionary<string, (string, string)>
            {
                { "Hardy",   ("Atk","Atk") }, { "Lonely",  ("Atk","Def") },
                { "Brave",   ("Atk","Spe") }, { "Adamant", ("Atk","SpA") },
                { "Naughty", ("Atk","SpD") }, { "Bold",    ("Def","Atk") },
                { "Docile",  ("Def","Def") }, { "Relaxed", ("Def","Spe") },
                { "Impish",  ("Def","SpA") }, { "Lax",     ("Def","SpD") },
                { "Timid",   ("Spe","Atk") }, { "Hasty",   ("Spe","Def") },
                { "Serious", ("Spe","Spe") }, { "Jolly",   ("Spe","SpA") },
                { "Naive",   ("Spe","SpD") }, { "Modest",  ("SpA","Atk") },
                { "Mild",    ("SpA","Def") }, { "Quiet",   ("SpA","Spe") },
                { "Bashful", ("SpA","SpA") }, { "Rash",    ("SpA","SpD") },
                { "Calm",    ("SpD","Atk") }, { "Gentle",  ("SpD","Def") },
                { "Sassy",   ("SpD","Spe") }, { "Careful", ("SpD","SpA") },
                { "Quirky",  ("SpD","SpD") },
            };

        private double GetNatureMult(string stat)
        {
            if (Nature == null || !NatureMap.TryGetValue(Nature, out var pair)) return 1.0;
            if (pair.Boost == stat && pair.Boost != pair.Reduce) return 1.1;
            if (pair.Reduce == stat && pair.Boost != pair.Reduce) return 0.9;
            return 1.0;
        }

        private int CalcHP(int b, int ev, int iv)
            => (int)(((2.0 * b + iv + ev / 4) * Level / 100.0) + Level + 10);
        private int CalcStat(int b, int ev, int iv, double n)
            => (int)((((2.0 * b + iv + ev / 4) * Level / 100.0) + 5) * n);

        public int FinalHP => CalcHP(HP, EvHP, IvHP);
        public int FinalAtk => CalcStat(Atk, EvAtk, IvAtk, GetNatureMult("Atk"));
        public int FinalDef => CalcStat(Def, EvDef, IvDef, GetNatureMult("Def"));
        public int FinalSpA => CalcStat(SpA, EvSpA, IvSpA, GetNatureMult("SpA"));
        public int FinalSpD => CalcStat(SpD, EvSpD, IvSpD, GetNatureMult("SpD"));
        public int FinalSpe => CalcStat(Spe, EvSpe, IvSpe, GetNatureMult("Spe"));
    }
}