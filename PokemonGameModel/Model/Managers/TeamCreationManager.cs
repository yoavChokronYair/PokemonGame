using PokemonGame.Core.Config;
using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Enums;
using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.Managers
{
   
    public class PokemonCreationData
    {
        public string Name { get; set; } = string.Empty;
        public int PokedexId { get; set; }
        public string Type1 { get; set; } = "Normal";
        public string? Type2 { get; set; }
        public int Level { get; set; }
        public string Nature { get; set; } = "Serious";

        public int BaseHp { get; set; }
        public int BaseAtk { get; set; }
        public int BaseDef { get; set; }
        public int BaseSpAtk { get; set; }
        public int BaseSpDef { get; set; }
        public int BaseSpeed { get; set; }

        public int IvHp { get; set; }
        public int IvAtk { get; set; }
        public int IvDef { get; set; }
        public int IvSpAtk { get; set; }
        public int IvSpDef { get; set; }
        public int IvSpeed { get; set; }

        public int EvHp { get; set; }
        public int EvAtk { get; set; }
        public int EvDef { get; set; }
        public int EvSpAtk { get; set; }
        public int EvSpDef { get; set; }
        public int EvSpeed { get; set; }

        public List<IMove> Moves { get; set; } = new();
        public IAbility Ability { get; set; } = null!;
        public IHeldItem? HeldItem { get; set; }
    }
    public class TeamCreationManager
    {
        public PokemonTeam BuildTeam(IReadOnlyList<PokemonCreationData> roster)
        {
            if (roster == null || roster.Count == 0)
                throw new InvalidOperationException("Cannot build a team from an empty roster.");

            if (roster.Count > PokemonConstants.PartyCapacity)
                throw new InvalidOperationException(
                    $"Expected maximum {PokemonConstants.PartyCapacity} members, found {roster.Count}.");

            var states = roster.Select(BuildPokemon).ToList();
            return PokemonTeam.Create(states);
        }

        public PokemonState BuildPokemon(PokemonCreationData data)
        {
            var nature = ParseEnum<NatureType>(data.Nature ?? "Serious");
            var mods = NatureConstants.GetNatureModifiers(nature);
            var modifiers = BuildModifierDict(mods);

            int maxHp = PokemonStatCalculatorHelper.CalculateHP(
                data.BaseHp, data.IvHp, data.EvHp, data.Level);

            return new PokemonState
            {
                Name = data.Name,
                PokedexId = data.PokedexId,
                PrimaryType = ParseEnum<PokemonType>(data.Type1),
                SecondaryType = data.Type2 != null ? ParseEnum<PokemonType>(data.Type2) : null,
                Level = data.Level,
                Nature = nature,
                MaxHP = maxHp,

                BaseAttack = CalcStat(data.BaseAtk, data.IvAtk, data.EvAtk, data.Level, modifiers, Stat.Attack),
                BaseDefense = CalcStat(data.BaseDef, data.IvDef, data.EvDef, data.Level, modifiers, Stat.Defense),
                BaseSpecialAttack = CalcStat(data.BaseSpAtk, data.IvSpAtk, data.EvSpAtk, data.Level, modifiers, Stat.SpecialAttack),
                BaseSpecialDefense = CalcStat(data.BaseSpDef, data.IvSpDef, data.EvSpDef, data.Level, modifiers, Stat.SpecialDefense),
                BaseSpeed = CalcStat(data.BaseSpeed, data.IvSpeed, data.EvSpeed, data.Level, modifiers, Stat.Speed),

                IVs = new[] { data.IvHp, data.IvAtk, data.IvDef, data.IvSpAtk, data.IvSpDef, data.IvSpeed },
                EVs = new[] { data.EvHp, data.EvAtk, data.EvDef, data.EvSpAtk, data.EvSpDef, data.EvSpeed },

                Moves = data.Moves,
                Ability = data.Ability,
                HeldItem = data.HeldItem,
            };
        }


        // ── Stat helpers ─────────────────────────────────────────────────────

        private static int CalcStat(int @base, int iv, int ev, int level,
                                    Dictionary<Stat, double> modifiers, Stat stat)
        {
            double mod = modifiers.TryGetValue(stat, out double m) ? m : 1.0;
            return PokemonStatCalculatorHelper.CalculateStat(@base, iv, ev, level, mod);
        }

        private static Dictionary<Stat, double> BuildModifierDict(
            (double atk, double def, double spAtk, double spDef, double speed) mods)
        {
            return new Dictionary<Stat, double>
            {
                { Stat.Attack,         mods.atk   },
                { Stat.Defense,        mods.def   },
                { Stat.SpecialAttack,  mods.spAtk },
                { Stat.SpecialDefense, mods.spDef },
                { Stat.Speed,          mods.speed }
            };
        }

        private static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum =>
            Enum.TryParse<TEnum>(value, true, out var result) ? result : default;
    }
}

