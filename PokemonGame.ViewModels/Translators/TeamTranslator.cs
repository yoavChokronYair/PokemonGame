using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Services.Handler;

namespace PokemonGame.ViewModels.Translators
{
    public class TeamTranslator
    {
        private readonly IPokemonService _pokemonService;
        private readonly MoveTranslator _moveTranslator;
        private readonly AbilityTranslator _abilityTranslator;
        private readonly ItemTranslator _itemTranslator;
        private readonly TeamCreationManager _teamCreator;

        public TeamTranslator()
        {
            _moveTranslator = new MoveTranslator();
            _abilityTranslator = new AbilityTranslator(new AbilityService(), _moveTranslator);
            _itemTranslator = new ItemTranslator(new ItemService(), _moveTranslator);
            _pokemonService = new PokemonService();
            _teamCreator = new TeamCreationManager();
        }

        public TeamTranslator(IPokemonService pokemonService, MoveTranslator moveTranslator,
                              AbilityTranslator abilityTranslator, ItemTranslator itemTranslator)
        {
            _pokemonService = pokemonService;
            _moveTranslator = moveTranslator;
            _abilityTranslator = abilityTranslator;
            _itemTranslator = itemTranslator;
            _teamCreator = new TeamCreationManager();
        }

        public PokemonTeam LoadTeam(int battlePlayerId)
        {
            var results = _pokemonService.LoadTeamResults(battlePlayerId);

            if (results == null || results.Count == 0)
                throw new InvalidOperationException($"No team found for Player ID {battlePlayerId}.");

            var roster = results.Select(ToCreationData).ToList();
            return _teamCreator.BuildTeam(roster);
        }

        public PokemonState TranslateToDomain(PokemonLoadResult result) =>
            _teamCreator.BuildPokemon(ToCreationData(result));

        // ── Mapping ──────────────────────────────────────────────────────────
        private PokemonCreationData ToCreationData(PokemonLoadResult result)
        {
            var b = result.Battler;
            var g = result.General;
            var s = result.Stats;

            return new PokemonCreationData
            {
                Name = g.Name ?? "MissingNo",
                PokedexId = g.PokedexID,
                Type1 = g.Type1 ?? "Normal",
                Type2 = g.Type2,
                Level = b.Level,
                Nature = b.Nature ?? "Serious",

                BaseHp = s.HP,
                BaseAtk = s.Attack,
                BaseDef = s.Defense,
                BaseSpAtk = s.SpAtk,
                BaseSpDef = s.SpDef,
                BaseSpeed = s.Speed,

                IvHp = b.Iv_hp,
                IvAtk = b.Iv_atk,
                IvDef = b.Iv_def,
                IvSpAtk = b.Iv_spAtk,
                IvSpDef = b.Iv_spDef,
                IvSpeed = b.Iv_speed,

                EvHp = b.Ev_hp,
                EvAtk = b.Ev_atk,
                EvDef = b.Ev_def,
                EvSpAtk = b.Ev_spAtk,
                EvSpDef = b.Ev_spDef,
                EvSpeed = b.Ev_speed,

                Moves = result.MoveNames
                               .Where(m => !string.IsNullOrEmpty(m))
                               .Select(BuildMove)
                               .ToList(),
                Ability = _abilityTranslator.TranslateById(b.AbilityID),
                HeldItem = BuildHeldItem(b.ItemID),
            };
        }
        // ── Translation helpers ──────────────────────────────────────────────
        private IMove BuildMove(string moveName)
        {
            return _moveTranslator.Translate(moveName);
        }
        private HeldItemState? BuildHeldItem(int? itemId)
        {
            if (itemId == null || itemId <= 0)
                return null;

            return _itemTranslator.TranslateById(itemId.Value);
        }
    }
}