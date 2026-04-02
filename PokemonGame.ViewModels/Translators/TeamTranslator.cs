using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Enums;
using PokemonGame.Model.Domain;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model;
using PokemonGame.Model.Model.Helper;
using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Model.Model.Helper.MoveHelper;
using PokemonGame.Services.Handler;

namespace PokemonGame.ViewModels.Translators
{
    public class TeamTranslator
    {
        private readonly IPokemonService _pokemonService;
        private readonly MoveTranslator _moveTranslator;
        private readonly AbilityTranslator _abilityTranslator;
        private readonly ItemTranslator _itemTranslator;

        public TeamTranslator()
        {
            _pokemonService = new PokemonService();
            _moveTranslator = new MoveTranslator();
            _abilityTranslator = new AbilityTranslator(new AbilityService(), _moveTranslator);
            _itemTranslator = new ItemTranslator(new ItemService(), _moveTranslator);
        }

        public TeamTranslator(IPokemonService pokemonService, MoveTranslator moveTranslator,
                              AbilityTranslator abilityTranslator, ItemTranslator itemTranslator)
        {
            _pokemonService = pokemonService;
            _moveTranslator = moveTranslator;
            _abilityTranslator = abilityTranslator;
            _itemTranslator = itemTranslator;
        }

        public PokemonTeam LoadTeam(int battlePlayerId)
        {
            var results = _pokemonService.LoadTeamResults(battlePlayerId);

            if (results == null || results.Count == 0)
                throw new InvalidOperationException($"No team found for Player ID {battlePlayerId}.");

            if (results.Count > 6)
                throw new InvalidOperationException($"Expected maximum 6 members, found {results.Count}.");

            var roster = results.Select(TranslateToDomain).ToList();
            return PokemonTeam.Create(roster);
        }

        public PokemonDomain TranslateToDomain(PokemonLoadResult result)
        {
            var b = result.Battler;
            var g = result.General;
            var s = result.Stats;

            var nature = ParseEnum<NatureType>(b.Nature ?? "Serious");
            var mods = NatureHelper.GetNatureModifiers(nature);

            var modifierDict = new Dictionary<Stat, double>
            {
                { Stat.Attack,          mods.atk   },
                { Stat.Defense,         mods.def   },
                { Stat.SpecialAttack,   mods.spAtk },
                { Stat.SpecialDefense,  mods.spDef },
                { Stat.Speed,           mods.speed }
            };

            int maxHp = PokemonStatCalculatorHelper.CalculateHP(s.HP, b.Iv_hp, b.Ev_hp, b.Level);

            return new PokemonDomain
            {
                Name = g.Name ?? "MissingNo",
                PokedexNumber = g.PokedexID,
                PrimaryType = ParseEnum<PokemonType>(g.Type1 ?? "Normal"),
                SecondaryType = g.Type2 != null ? ParseEnum<PokemonType>(g.Type2) : (PokemonType?)null,
                Level = b.Level,
                Nature = nature,
                MaxHP = maxHp,
                CurrentHP = maxHp,

                BaseAttack = CalcStat(s.Attack, b.Iv_atk, b.Ev_atk, b.Level, modifierDict, Stat.Attack),
                BaseDefense = CalcStat(s.Defense, b.Iv_def, b.Ev_def, b.Level, modifierDict, Stat.Defense),
                BaseSpecialAttack = CalcStat(s.SpAtk, b.Iv_spAtk, b.Ev_spAtk, b.Level, modifierDict, Stat.SpecialAttack),
                BaseSpecialDefense = CalcStat(s.SpDef, b.Iv_spDef, b.Ev_spDef, b.Level, modifierDict, Stat.SpecialDefense),
                BaseSpeed = CalcStat(s.Speed, b.Iv_speed, b.Ev_speed, b.Level, modifierDict, Stat.Speed),

                IVs = new[] { b.Iv_hp, b.Iv_atk, b.Iv_def, b.Iv_spAtk, b.Iv_spDef, b.Iv_speed },
                EVs = new[] { b.Ev_hp, b.Ev_atk, b.Ev_def, b.Ev_spAtk, b.Ev_spDef, b.Ev_speed },

                Moves = result.MoveNames.Where(m => !string.IsNullOrEmpty(m)).Select(BuildMove).ToList(),
                Ability = BuildAbility(b.AbilityID),
                HeldItem = BuildHeldItem(b.ItemID),
            };
        }

        private AbilityState BuildAbility(int abilityId) =>
            _abilityTranslator.TranslateById(abilityId);

        // Returns null if the pokemon has no item
        private HeldItemState? BuildHeldItem(int? itemId)
        {
            if (itemId == null || itemId <= 0) return null;
            return _itemTranslator.TranslateById(itemId.Value);
        }

        private IMove BuildMove(string moveName)
        {
            var domain = _moveTranslator.Translate(moveName);
            var attempt = _moveTranslator.TranslateAttemptForMove(moveName);
            return new MoveState(domain, attempt);
        }

        private static int CalcStat(int @base, int iv, int ev, int level,
                                    Dictionary<Stat, double> modifiers, Stat stat)
        {
            double mod = modifiers.TryGetValue(stat, out double m) ? m : 1.0;
            return PokemonStatCalculatorHelper.CalculateStat(@base, iv, ev, level, mod);
        }

        private static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum =>
            Enum.TryParse<TEnum>(value, true, out var result) ? result : default;
    }
}