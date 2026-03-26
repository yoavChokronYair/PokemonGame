using System.Collections.ObjectModel;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelUserControl
{
    public class PokemonType
    {
        public string Name { get; set; }
        public SolidColorBrush Color { get; set; }
    }

    public class PokemonEntry
    {
        public int PokedexId { get; set; }
        public string Name { get; set; }
        public List<PokemonType> Types { get; set; } = new();
        public string AbilityPrimary { get; set; }
        public string? AbilitySecondary { get; set; }
        public string? AbilityHidden { get; set; }
        public int HP { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int SpA { get; set; }
        public int SpD { get; set; }
        public int Spe { get; set; }
        public int BST => HP + Atk + Def + SpA + SpD + Spe;
    }

    // ── Type color palette ────────────────────────────────────────────────────

    public static class TypeColors
    {
        private static readonly Dictionary<string, Color> _map = new()
        {
            { "NORMAL",   Color.FromRgb(168, 168, 120) },
            { "FIRE",     Color.FromRgb(240, 128,  48) },
            { "WATER",    Color.FromRgb( 104, 144, 240) },
            { "ELECTRIC", Color.FromRgb(248, 208,  48) },
            { "GRASS",    Color.FromRgb(120, 200,  80) },
            { "ICE",      Color.FromRgb(152, 216, 216) },
            { "FIGHTING", Color.FromRgb(192,  48,  40) },
            { "POISON",   Color.FromRgb(160,  65, 160) },
            { "GROUND",   Color.FromRgb(224, 192,  80) },
            { "FLYING",   Color.FromRgb(168, 144, 240) },
            { "PSYCHIC",  Color.FromRgb(248,  88, 136) },
            { "BUG",      Color.FromRgb(168, 184,  32) },
            { "ROCK",     Color.FromRgb(184, 160,  56) },
            { "GHOST",    Color.FromRgb(112,  88, 152) },
            { "DRAGON",   Color.FromRgb(112,  56, 248) },
            { "DARK",     Color.FromRgb(112,  88,  72) },
            { "STEEL",    Color.FromRgb(184, 184, 208) },
            { "FAIRY",    Color.FromRgb(238, 153, 172) },
        };

        public static SolidColorBrush Get(string type)
        {
            var key = type.ToUpperInvariant();
            var color = _map.TryGetValue(key, out var c) ? c : Color.FromRgb(180, 180, 180);
            return new SolidColorBrush(color);
        }

        public static PokemonType Make(string name) => new()
        {
            Name = name.ToUpperInvariant(),
            Color = Get(name)
        };
    }

    // ── ViewModel ─────────────────────────────────────────────────────────────

    public class PokemonTableViewModel : ViewModelBase
    {
        // ── List ──────────────────────────────────────────────────────────────

        public ObservableCollection<PokemonEntry> PokemonList { get; } = new(BuildList());

        // ── Selection ─────────────────────────────────────────────────────────

        private PokemonEntry? _selectedPokemon;
        public PokemonEntry? SelectedPokemon
        {
            get => _selectedPokemon;
            set => SetProperty(ref _selectedPokemon, value);
        }

        // ── Static data ───────────────────────────────────────────────────────

        private static List<PokemonEntry> BuildList() => new()
        {
            new PokemonEntry
            {
                PokedexId = 594, Name = "Alomomola",
                Types = new List<PokemonType> { TypeColors.Make("WATER") },
                AbilityPrimary = "Healer", AbilitySecondary = "Hydration", AbilityHidden = "Regenerator",
                HP = 165, Atk = 75, Def = 80, SpA = 40, SpD = 45, Spe = 65
            },
            new PokemonEntry
            {
                PokedexId = 903, Name = "Ceruledge",
                Types = new List<PokemonType> { TypeColors.Make("FIRE"), TypeColors.Make("GHOST") },
                AbilityPrimary = "Flash Fire", AbilitySecondary = null, AbilityHidden = "Weak Armor",
                HP = 75, Atk = 125, Def = 80, SpA = 60, SpD = 100, Spe = 85
            },
            new PokemonEntry
            {
                PokedexId = 815, Name = "Cinderace",
                Types = new List<PokemonType> { TypeColors.Make("FIRE") },
                AbilityPrimary = "Blaze", AbilitySecondary = null, AbilityHidden = "Libero",
                HP = 80, Atk = 116, Def = 75, SpA = 65, SpD = 75, Spe = 119
            },
            new PokemonEntry
            {
                PokedexId = 35, Name = "Clefable",
                Types = new List<PokemonType> { TypeColors.Make("FAIRY") },
                AbilityPrimary = "Cute Charm", AbilitySecondary = "Magic Guard", AbilityHidden = "Unaware",
                HP = 95, Atk = 70, Def = 73, SpA = 95, SpD = 90, Spe = 60
            },
            new PokemonEntry
            {
                PokedexId = 823, Name = "Corviknight",
                Types = new List<PokemonType> { TypeColors.Make("FLYING"), TypeColors.Make("STEEL") },
                AbilityPrimary = "Pressure", AbilitySecondary = "Unnerve", AbilityHidden = "Mirror Armor",
                HP = 98, Atk = 87, Def = 105, SpA = 53, SpD = 85, Spe = 67
            },
            new PokemonEntry
            {
                PokedexId = 491, Name = "Darkrai",
                Types = new List<PokemonType> { TypeColors.Make("DARK") },
                AbilityPrimary = "Bad Dreams", AbilitySecondary = null, AbilityHidden = null,
                HP = 70, Atk = 90, Def = 90, SpA = 135, SpD = 90, Spe = 125
            },
            new PokemonEntry
            {
                PokedexId = 386, Name = "Deoxys-Speed",
                Types = new List<PokemonType> { TypeColors.Make("PSYCHIC") },
                AbilityPrimary = "Pressure", AbilitySecondary = null, AbilityHidden = null,
                HP = 50, Atk = 95, Def = 90, SpA = 95, SpD = 90, Spe = 180
            },
            new PokemonEntry
            {
                PokedexId = 980, Name = "Dondozo",
                Types = new List<PokemonType> { TypeColors.Make("WATER") },
                AbilityPrimary = "Unaware", AbilitySecondary = "Oblivious", AbilityHidden = "Water Veil",
                HP = 150, Atk = 100, Def = 115, SpA = 65, SpD = 65, Spe = 35
            },
            new PokemonEntry
            {
                PokedexId = 887, Name = "Dragapult",
                Types = new List<PokemonType> { TypeColors.Make("DRAGON"), TypeColors.Make("GHOST") },
                AbilityPrimary = "Clear Body", AbilitySecondary = "Infiltrator", AbilityHidden = "Cursed Body",
                HP = 88, Atk = 120, Def = 75, SpA = 100, SpD = 75, Spe = 142
            },
            new PokemonEntry
            {
                PokedexId = 149, Name = "Dragonite",
                Types = new List<PokemonType> { TypeColors.Make("DRAGON"), TypeColors.Make("FLYING") },
                AbilityPrimary = "Inner Focus", AbilitySecondary = null, AbilityHidden = "Multiscale",
                HP = 91, Atk = 134, Def = 95, SpA = 100, SpD = 100, Spe = 80
            },
            new PokemonEntry
            {
                PokedexId = 641, Name = "Enamorus",
                Types = new List<PokemonType> { TypeColors.Make("FAIRY"), TypeColors.Make("FLYING") },
                AbilityPrimary = "Cute Charm", AbilitySecondary = null, AbilityHidden = "Contrary",
                HP = 74, Atk = 115, Def = 70, SpA = 135, SpD = 80, Spe = 106
            },
        };
    }
}
