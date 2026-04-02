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

        /// <summary>
        /// Default constructor using standard service implementations.
        /// </summary>
        public TeamTranslator()
        {
            _pokemonService = new PokemonService();
            _moveTranslator = new MoveTranslator();
            // Assuming AbilityService is the standard implementation for IAbilityService
            _abilityTranslator = new AbilityTranslator(new AbilityService(), _moveTranslator);
        }

        /// <summary>
        /// Injection constructor for testing or custom service configurations.
        /// </summary>
        public TeamTranslator(IPokemonService pokemonService, MoveTranslator moveTranslator, AbilityTranslator abilityTranslator)
        {
            _pokemonService = pokemonService;
            _moveTranslator = moveTranslator;
            _abilityTranslator = abilityTranslator;
        }

        public PokemonTeam LoadTeam(int battlePlayerId)
        {
            // The service handles all DB coordination (Joining Battler, General, and Stats)
            var results = _pokemonService.LoadTeamResults(battlePlayerId);

            if (results == null || results.Count == 0)
            {
                throw new InvalidOperationException($"No team found for Player ID {battlePlayerId}.");
            }

            // Standard Pokémon team constraint (usually 1-6)
            if (results.Count > 6)
            {
                throw new InvalidOperationException($"Expected maximum 6 members, found {results.Count}.");
            }

            // Transform data results into Domain objects
            var roster = results
                .Select(TranslateToDomain)
                .ToList();

            return PokemonTeam.Create(roster);
        }

        public PokemonDomain TranslateToDomain(PokemonLoadResult result)
        {
            var b = result.Battler;
            var g = result.General;
            var s = result.Stats;

            // 1. Resolve Nature and Modifiers
            var nature = ParseEnum<NatureType>(b.Nature ?? "Serious");
            var mods = NatureHelper.GetNatureModifiers(nature);

            var modifierDict = new Dictionary<Stat, double>
            {
                { Stat.Attack, mods.atk },
                { Stat.Defense, mods.def },
                { Stat.SpecialAttack, mods.spAtk },
                { Stat.SpecialDefense, mods.spDef },
                { Stat.Speed, mods.speed }
            };

            // 2. Calculate HP
            int maxHp = PokemonStatCalculatorHelper.CalculateHP(s.HP, b.Iv_hp, b.Ev_hp, b.Level);

            // 3. Build the Domain Object
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

                // Stat calculations using the helper and modifiers
                BaseAttack = CalcStat(s.Attack, b.Iv_atk, b.Ev_atk, b.Level, modifierDict, Stat.Attack),
                BaseDefense = CalcStat(s.Defense, b.Iv_def, b.Ev_def, b.Level, modifierDict, Stat.Defense),
                BaseSpecialAttack = CalcStat(s.SpAtk, b.Iv_spAtk, b.Ev_spAtk, b.Level, modifierDict, Stat.SpecialAttack),
                BaseSpecialDefense = CalcStat(s.SpDef, b.Iv_spDef, b.Ev_spDef, b.Level, modifierDict, Stat.SpecialDefense),
                BaseSpeed = CalcStat(s.Speed, b.Iv_speed, b.Ev_speed, b.Level, modifierDict, Stat.Speed),

                // Array mappings for bulk processing
                IVs = new[] { b.Iv_hp, b.Iv_atk, b.Iv_def, b.Iv_spAtk, b.Iv_spDef, b.Iv_speed },
                EVs = new[] { b.Ev_hp, b.Ev_atk, b.Ev_def, b.Ev_spAtk, b.Ev_spDef, b.Ev_speed },

                // Functional Logic Objects (Moves & Ability)
                Moves = result.MoveNames.Where(m => !string.IsNullOrEmpty(m)).Select(BuildMove).ToList(),
                Ability = BuildAbility(b.AbilityID)
            };
        }

        private AbilityState BuildAbility(int abilityId)
        {
            return _abilityTranslator.TranslateById(abilityId);
          
        }

        private IMove BuildMove(string moveName)
        {
            var domain = _moveTranslator.Translate(moveName);
            var attempt = _moveTranslator.TranslateAttemptForMove(moveName);
            return new MoveState(domain, attempt);
        }

        private static int CalcStat(int @base, int iv, int ev, int level, Dictionary<Stat, double> modifiers, Stat stat)
        {
            double mod = modifiers.TryGetValue(stat, out double m) ? m : 1.0;
            return PokemonStatCalculatorHelper.CalculateStat(@base, iv, ev, level, mod);
        }

        private static TEnum ParseEnum<TEnum>(string value) where TEnum : struct, Enum
        {
            return Enum.TryParse<TEnum>(value, true, out var result) ? result : default;
        }
    }
}