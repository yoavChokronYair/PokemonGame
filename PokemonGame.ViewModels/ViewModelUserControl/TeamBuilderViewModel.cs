using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelUserControl;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class TeamBuilderViewModel : ViewModelBase
    {
        public TeamBuilderState State { get; }
        public TeamSlotBarViewModel SlotBar { get; }
        public PokemonEditorViewModel Editor { get; }
        public MovePickerViewModel MovePicker { get; }
        public ItemPickerViewModel ItemPicker { get; }
        public PokemonPickerViewModel PokemonPicker { get; }
        public EvIvEditorViewModel EvIvEditor { get; }
        public TeamManagementViewModel TeamManagement { get; }

        public TeamBuilderViewModel(UserStore userStore)
        {
            var service = new TeamBuilderService();
            State = new TeamBuilderState();
            var allPokemon = new ObservableCollection<PokemonDisplayEntry>(service.GetAllPokemon());
            var allItems = new ObservableCollection<ItemData>(service.GetHeldItems());

            SlotBar = new TeamSlotBarViewModel(State, service);
            Editor = new PokemonEditorViewModel(State, service);
            MovePicker = new MovePickerViewModel(State, service);
            ItemPicker = new ItemPickerViewModel(State, service,allItems);
            PokemonPicker = new PokemonPickerViewModel(State, service,allPokemon);
            EvIvEditor = new EvIvEditorViewModel(State);
            TeamManagement = new TeamManagementViewModel(State, service, userStore, allPokemon, allItems);
        }
    }
    // ── TeamSlotEntry ─────────────────────────────────────────────────────────
    // Wraps PokemonDisplayEntry (read-only template data) with all observable
    // editable fields the editor card binds to.

    public class TeamSlotEntry : ViewModelBase
    {
        // ── Template data (read-only, from DB) ────────────────────────────────
        public int PokedexId { get; }
        public string Name { get; }
        public string Type1 { get; }
        public string? Type2 { get; }
        public List<string> Abilities { get; }
        public List<MoveDisplayEntry> AvailableMoves { get; }
        public int HP { get; }
        public int Atk { get; }
        public int Def { get; }
        public int SpA { get; }
        public int SpD { get; }
        public int Spe { get; }
        public int BST => HP + Atk + Def + SpA + SpD + Spe;
        public BitmapImage SpriteImage { get; }
        public List<string> Types { get; }

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
            SpriteImage = src.SpriteImage;
            Types = src.Types;
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
        public string SuggestedNature
        {
            get
            {
                // Determine which offensive stat is being invested in
                bool physicalAttacker = EvAtk >= 252;
                bool specialAttacker = EvSpA >= 252;
                bool speedInvested = EvSpe >= 252;
                bool slowRole = IvSpe == 0; // trick room

                if (physicalAttacker && speedInvested) return "Adamant or Jolly";
                if (physicalAttacker && slowRole) return "Brave (Trick Room)";
                if (physicalAttacker) return "Adamant";
                if (specialAttacker && speedInvested) return "Modest or Timid";
                if (specialAttacker && slowRole) return "Quiet (Trick Room)";
                if (specialAttacker) return "Modest";
                if (speedInvested) return "Jolly or Timid";
                if (EvHP >= 252) return "Bold or Calm (Defensive)";
                return "—";
            }
        }

        // ── Moves ─────────────────────────────────────────────────────────────
        private MoveDisplayEntry _move1;
        public MoveDisplayEntry Move1
        {
            get => _move1;
            set { if (SetProperty(ref _move1, value)) OnPropertyChanged(nameof(Move1Display)); }
        }

        private MoveDisplayEntry _move2;
        public MoveDisplayEntry Move2
        {
            get => _move2;
            set { if (SetProperty(ref _move2, value)) OnPropertyChanged(nameof(Move2Display)); }
        }

        private MoveDisplayEntry _move3;
        public MoveDisplayEntry Move3
        {
            get => _move3;
            set { if (SetProperty(ref _move3, value)) OnPropertyChanged(nameof(Move3Display)); }
        }

        private MoveDisplayEntry _move4;
        public MoveDisplayEntry Move4
        {
            get => _move4;
            set { if (SetProperty(ref _move4, value)) OnPropertyChanged(nameof(Move4Display)); }
        }

        // Display-only name strings for the editor card buttons
        public string Move1Display => Move1?.Name ?? "— Move 1 —";
        public string Move2Display => Move2?.Name ?? "— Move 2 —";
        public string Move3Display => Move3?.Name ?? "— Move 3 —";
        public string Move4Display => Move4?.Name ?? "— Move 4 —";

        // Replace all 6 EV properties with capped versions:
        // ── EVs ───────────────────────────────────────────────────────────────────
        private int _evHP;
        public int EvHP
        {
            get => _evHP;
            set
            {
                int clamped = Math.Min(252, Math.Max(0, Math.Min(value, 510 - (EvAtk + EvDef + EvSpA + EvSpD + EvSpe))));
                if (SetProperty(ref _evHP, clamped))
                {
                    OnPropertyChanged(nameof(FinalHP));
                    OnPropertyChanged(nameof(RemainingEvs));
                    OnPropertyChanged(nameof(SuggestedNature));
                }
            }
        }
        
        private int _evAtk;
        public int EvAtk    
        {
            get => _evAtk;
            set
            {
                int clamped = Math.Min(252, Math.Max(0, Math.Min(value, 510 - (EvHP + EvDef + EvSpA + EvSpD + EvSpe))));
                if (SetProperty(ref _evAtk, clamped))
                {
                    OnPropertyChanged(nameof(FinalAtk));
                    OnPropertyChanged(nameof(RemainingEvs));
                    OnPropertyChanged(nameof(SuggestedNature));
                }
            }
        }

        private int _evDef;
        public int EvDef
        {
            get => _evDef;
            set
            {
                int clamped = Math.Min(252, Math.Max(0, Math.Min(value, 510 - (EvHP + EvAtk + EvSpA + EvSpD + EvSpe))));
                if (SetProperty(ref _evDef, clamped))
                {
                    OnPropertyChanged(nameof(FinalDef));
                    OnPropertyChanged(nameof(RemainingEvs));
                    OnPropertyChanged(nameof(SuggestedNature));
                }
            }
        }

        private int _evSpA;
        public int EvSpA
        {
            get => _evSpA;
            set
            {
                int clamped = Math.Min(252, Math.Max(0, Math.Min(value, 510 - (EvHP + EvAtk + EvDef + EvSpD + EvSpe))));
                if (SetProperty(ref _evSpA, clamped))
                {
                   
                    OnPropertyChanged(nameof(FinalSpA));
                    OnPropertyChanged(nameof(RemainingEvs));
                    OnPropertyChanged(nameof(SuggestedNature));
                }
            }
        }

        private int _evSpD;
        public int EvSpD
        {
            get => _evSpD;
            set
            {
                int clamped = Math.Min(252, Math.Max(0, Math.Min(value, 510 - (EvHP + EvAtk + EvDef + EvSpA + EvSpe))));
                if (SetProperty(ref _evSpD, clamped))
                {
                    OnPropertyChanged(nameof(FinalSpD));
                    OnPropertyChanged(nameof(RemainingEvs));
                    OnPropertyChanged(nameof(SuggestedNature));
                }
            }
        }

        private int _evSpe;
        public int EvSpe
        {
            get => _evSpe;
            set
            {
                int clamped = Math.Min(252, Math.Max(0, Math.Min(value, 510 - (EvHP + EvAtk + EvDef + EvSpA + EvSpD))));
                if (SetProperty(ref _evSpe, clamped))
                {
                    OnPropertyChanged(nameof(FinalSpe));
                    OnPropertyChanged(nameof(RemainingEvs));
                    OnPropertyChanged(nameof(SuggestedNature));
                }
            }
        }

        // ── IVs ───────────────────────────────────────────────────────────────────
        private int _ivHP = 31;
        public int IvHP
        {
            get => _ivHP;
            set { if (SetProperty(ref _ivHP, value)) { OnPropertyChanged(nameof(FinalHP)); OnPropertyChanged(nameof(SuggestedNature)); } }
        }

        private int _ivAtk = 31;
        public int IvAtk
        {
            get => _ivAtk;
            set { if (SetProperty(ref _ivAtk, value)) { OnPropertyChanged(nameof(FinalAtk)); OnPropertyChanged(nameof(SuggestedNature)); } }
        }

        private int _ivDef = 31;
        public int IvDef
        {
            get => _ivDef;
            set { if (SetProperty(ref _ivDef, value)) { OnPropertyChanged(nameof(FinalDef)); OnPropertyChanged(nameof(SuggestedNature)); } }
        }

        private int _ivSpA = 31;
        public int IvSpA
        {
            get => _ivSpA;
            set { if (SetProperty(ref _ivSpA, value)) { OnPropertyChanged(nameof(FinalSpA)); OnPropertyChanged(nameof(SuggestedNature)); } }
        }

        private int _ivSpD = 31;
        public int IvSpD
        {
            get => _ivSpD;
            set { if (SetProperty(ref _ivSpD, value)) { OnPropertyChanged(nameof(FinalSpD)); OnPropertyChanged(nameof(SuggestedNature)); } }
        }

        private int _ivSpe = 31;
        public int IvSpe
        {
            get => _ivSpe;
            set { if (SetProperty(ref _ivSpe, value)) { OnPropertyChanged(nameof(FinalSpe)); OnPropertyChanged(nameof(SuggestedNature)); } }
        }

        // ── IV Spread presets ─────────────────────────────────────────────────────
        public void ApplyIvSpread(string spread)
        {
            switch (spread)
            {
                case "max all": IvHP = 31; IvAtk = 31; IvDef = 31; IvSpA = 31; IvSpD = 31; IvSpe = 31; break;
                case "min Atk": IvHP = 31; IvAtk = 0; IvDef = 31; IvSpA = 31; IvSpD = 31; IvSpe = 31; break;
                case "min Atk, min Spe": IvHP = 31; IvAtk = 0; IvDef = 31; IvSpA = 31; IvSpD = 31; IvSpe = 0; break;
                case "min Spe": IvHP = 31; IvAtk = 31; IvDef = 31; IvSpA = 31; IvSpD = 31; IvSpe = 0; break;
                case "min all": IvHP = 0; IvAtk = 0; IvDef = 0; IvSpA = 0; IvSpD = 0; IvSpe = 0; break;
            }
            OnPropertyChanged(nameof(SuggestedNature));
        }

        public int RemainingEvs => 510 - EvHP - EvAtk - EvDef - EvSpA - EvSpD - EvSpe;

       

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