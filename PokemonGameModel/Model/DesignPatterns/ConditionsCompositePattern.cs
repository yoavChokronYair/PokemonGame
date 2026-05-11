// Design: Composite pattern — And/Or/Not combine conditions.
// Design: Specification pattern — each class encodes one named battle condition.
// UserCondition / OpponentCondition: Adapter pattern (wraps PokemonDomain condition for BattleDomain).
// Layer: Domain/Move — concrete condition implementations.
// ICondition<T> and ITarget interfaces live in Interface/Move/IConditionAndTarget.cs.
// NOTE: Probability uses RandomHelper — no inline new Random() anywhere in this file.

using PokemonGame.Core.Config;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.DesignPatterns
{
    public class IsBattleOver : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.IsBattleOver;
    }
    public class WasHitByCrit : ICondition<BattleState>
    {
        public bool Check(BattleState battle)
        {
            if (battle.LastUsedMove is MoveState lastMove)
            {

                return lastMove.CritStage == 1;
            }
            return false;
        }
    }
    // ── Condition Combinators ─────────────────────────────────────────────────
    public class And<T> : ICondition<T>
    {
        private readonly ICondition<T> _left;
        private readonly ICondition<T> _right;
        public And(ICondition<T> left, ICondition<T> right) { _left = left; _right = right; }
        public bool Check(T entity) => _left.Check(entity) && _right.Check(entity);
    }
    public class Or<T> : ICondition<T>
    {
        private readonly ICondition<T> _left;
        private readonly ICondition<T> _right;
        public Or(ICondition<T> left, ICondition<T> right) { _left = left; _right = right; }
        public bool Check(T entity) => _left.Check(entity) || _right.Check(entity);
    }

    public class Not<T> : ICondition<T>
    {
        private readonly ICondition<T> _inner;
        public Not(ICondition<T> inner) { _inner = inner; }
        public bool Check(T entity) => !_inner.Check(entity);
    }

    // ── Probability ───────────────────────────────────────────────────────────

    // Uses RandomHelper.NextBool — do not use new Random() here.
    public class Probability<T> : ICondition<T>
    {
        private readonly double _probability;
        public Probability(double probability)
        {
            _probability = MathHelper.Clamp(probability, 0.0, 1.0);
        }
        public bool Check(T entity) => RandomHelper.NextBool(_probability);
    }

    // ── Core Logic ───────────────────────────────────────────────────────────

    public class Probability : ICondition<BattleState>
    {
        private readonly double _probability;
        public Probability(double probability) { _probability = MathHelper.Clamp(probability, 0.0, 1.0); }
        public bool Check(BattleState battle) => RandomHelper.NextBool(_probability);
    }
    public class ProbabilityPokemon : ICondition<PokemonState>
    {
        private readonly double _probability;
        public ProbabilityPokemon(double probability) { _probability = MathHelper.Clamp(probability, 0.0, 1.0); }
        public bool Check(PokemonState battle) => RandomHelper.NextBool(_probability);
    }
    // ── Battle/Environment Conditions ─────────────────────────────────────────
    
    public class IsNewPokemon : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.Attacker.turnsActive == 0;
    }
    
    public class IsWeatherActive : ICondition<BattleState>
    {
        private readonly Weather _weather;
        public IsWeatherActive(Weather weather) { _weather = weather; }
        public bool Check(BattleState battle) => battle.WeatherService.IsWeatherActive(_weather);
    }

    public class IsTerrainActive : ICondition<BattleState>
    {
        private readonly TerrainType _terrain;
        public IsTerrainActive(TerrainType terrain) { _terrain = terrain; }
        public bool Check(BattleState battle) => battle.TerrainService.CurrentTerrain == _terrain;
    }

    public class IsAnyTerrainActive : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.TerrainService.CurrentTerrain != TerrainType.None;
    }
    // ── Move-Based Conditions (on Attacker's last move) ───────────────────────

    public class WasHitByContact : ICondition<BattleState>
    {
        public bool Check(BattleState battle)
        {
            if (battle.LastUsedMove is MoveState lastMove)
            {
                
                return lastMove.Category == MoveCategory.Physical;
            }
            return false;
        }
    }
    public class WasHitByMoveType : ICondition<BattleState>
    {
        private readonly PokemonType _moveType;
        public WasHitByMoveType(PokemonType moveName) { _moveType = moveName; }
        public bool Check(BattleState battle) =>
            battle.LastUsedMove is MoveState lastMove && lastMove.Element == _moveType;
    }

    public class MoveHasTag : ICondition<BattleState>
    {
        private readonly MoveTag _tag;
        public MoveHasTag(MoveTag tag) { _tag = tag; }
        public bool Check(BattleState battle) =>
            battle.LastUsedMove is MoveState lastMove && lastMove.Tag == _tag;
    }

    public class MoveIsCategory : ICondition<BattleState>
    {
        private readonly MoveCategory _category;
        public MoveIsCategory(MoveCategory category) { _category = category; }
        public bool Check(BattleState battle) =>
            battle.LastUsedMove is MoveState lastMove && lastMove.Category == _category;
    }

    // ── Attacker State Conditions ─────────────────────────────────────────────

    public class DidKnockoutOpponent : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.Defender.IsFainted && !battle.Attacker.IsFainted;
    }

    public class TookDamageThisTurn : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.Attacker.LastDamageTaken > 0;
    }

    public class HasAnyStatus : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.Attacker.PokemonStatusCondition() != StatusCondition.None;
    }

    public class HasStatus : ICondition<BattleState>
    {
        private readonly StatusCondition _status;
        public HasStatus(StatusCondition status) { _status = status; }
        public bool Check(BattleState battle) => battle.Attacker.PokemonStatusCondition() == _status;
    }

    public class IsHoldingItem : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.Attacker.HeldItem != null;
    }

    public class HasVolatile : ICondition<BattleState>
    {
        private readonly VolatileStatus _status;
        public HasVolatile(VolatileStatus status) { _status = status; }
        public bool Check(BattleState battle) => battle.Attacker.HasVolatileStatus(_status);
    }

    public class HasBaseStatChanged : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.Attacker.WasStatLoweredThisTurn;
    }
    public class IsFainted : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.Attacker.IsFainted;
    }
    public class IsFullHP : ICondition<BattleState>
    {
        public bool Check(BattleState battle) => battle.Attacker.CurrentHP == battle.Attacker.MaxHP;
    }
    public class HPBelow : ICondition<BattleState>
    {
        private readonly double _fraction;
        public HPBelow(double fraction) { _fraction = fraction; }
        public bool Check(BattleState battle) => battle.Attacker.GetHPFraction() < _fraction;
    }
    public class HasType : ICondition<BattleState>
    {
        private readonly PokemonType _type;
        public HasType(PokemonType type) { _type = type; }
        public bool Check(BattleState battle) => battle.Attacker.HasType(_type);
    }
    // Adapter: wraps a PokemonDomain condition so it can be used as ICondition<BattleDomain>.
    public class UserCondition : ICondition<BattleState>
    {
        private readonly ICondition<BattleState> _inner;
        public UserCondition(ICondition<BattleState> inner) { _inner = inner; }
        public bool Check(BattleState battle) => _inner.Check(battle);
    }
    public class IsGrounded : ICondition<BattleState>
    {
        public bool Check(BattleState battle) =>
            !battle.Defender.HasType(PokemonType.Flying) ||
            battle.Defender.HeldItem is HeldItemState item && item.Name == "Iron Ball" ||
            battle.Defender.HasVolatileStatus(VolatileStatus.Ingrain) ||
            battle.Defender.HasVolatileStatus(VolatileStatus.SmackDown) ||
            battle.IsGravityActive;
    }
    public class OpponentCondition : ICondition<BattleState>
    {
        private readonly ICondition<PokemonState> _inner;
        public OpponentCondition(ICondition<PokemonState> inner) { _inner = inner; }
        public bool Check(BattleState battle) => _inner.Check(battle.Defender); // ← fix
    }
    public class PokemonHasStatus : ICondition<PokemonState>
    {
        private readonly StatusCondition _status;
        public PokemonHasStatus(StatusCondition status) { _status = status; }
        public bool Check(PokemonState pokemon) => pokemon.PokemonStatusCondition() == _status;
    }

    public class PokemonHasVolatile : ICondition<PokemonState>
    {
        private readonly VolatileStatus _status;
        public PokemonHasVolatile(VolatileStatus status) { _status = status; }
        public bool Check(PokemonState pokemon) => pokemon.HasVolatileStatus(_status);
    }

    public class PokemonHasType : ICondition<PokemonState>
    {
        private readonly PokemonType _type;
        public PokemonHasType(PokemonType type) { _type = type; }
        public bool Check(PokemonState pokemon) => pokemon.HasType(_type);
    }

    public class PokemonHPBelow : ICondition<PokemonState>
    {
        private readonly double _fraction;
        public PokemonHPBelow(double fraction) { _fraction = fraction; }
        public bool Check(PokemonState pokemon) => pokemon.GetHPFraction() < _fraction;
    }

    public class PokemonIsFullHP : ICondition<PokemonState>
    {
        public bool Check(PokemonState pokemon) => pokemon.CurrentHP == pokemon.MaxHP;
    }

    public class PokemonIsFainted : ICondition<PokemonState>
    {
        public bool Check(PokemonState pokemon) => pokemon.IsFainted;
    }

    // status
    // ── Team conditions ───────────────────────────────────────────────────────────

    public class TeamHasPokemon : ICondition<PlayerDomain>
    {
        private readonly int _pokedexId;
        public TeamHasPokemon(int pokedexId) { _pokedexId = pokedexId; }
        public bool Check(PlayerDomain player) => player.Team.ContainsPokemon(_pokedexId);
    }

    public class TeamHasSpace : ICondition<PlayerDomain>
    {
        public bool Check(PlayerDomain player) => player.Team.getAllPokemonCount() < PokemonConstants.PartyCapacity;
    }

    // ── Inventory conditions ──────────────────────────────────────────────────────

    public class HasItem : ICondition<PlayerDomain>
    {
        private readonly itemsDomain _item;
        public HasItem(itemsDomain item) { _item = item; }
        public bool Check(PlayerDomain player) => player.trainerItemDomain.BagInventory.ContainsKey(_item);
    }

    public class HasEnoughMoney : ICondition<PlayerDomain>
    {
        private readonly int _amount;
        public HasEnoughMoney(int amount) { _amount = amount; }
        public bool Check(PlayerDomain player) => player.trainerInfo.Money >= _amount;
    }

    // ── Progress conditions ───────────────────────────────────────────────────────

    public class HasBadge : ICondition<PlayerDomain>
    {
        private readonly int _badgeId;
        public HasBadge(int badgeId) { _badgeId = badgeId; }
        public bool Check(PlayerDomain player) => player.HasBadge(_badgeId);
    }

    public class HasStoryFlag : ICondition<PlayerDomain>
    {
        private readonly int _flagId;
        public HasStoryFlag(int flagId) { _flagId = flagId; }
        public bool Check(PlayerDomain player) => player.HasStoryFlag(_flagId);
    }

    public class TrainerDefeated : ICondition<PlayerDomain>
    {
        private readonly int _trainerId;
        public TrainerDefeated(int trainerId) { _trainerId = trainerId; }
        public bool Check(PlayerDomain player) => player.HasDefeatedTrainer(_trainerId);
    }

    public class NotDefeatedCondition : ICondition<PlayerDomain>
    {
        private readonly int _trainerId;
        public NotDefeatedCondition(int trainerId) { _trainerId = trainerId; }
        public bool Check(PlayerDomain player) => !player.HasDefeatedTrainer(_trainerId);
    }

    public class ItemAlreadyTaken : ICondition<PlayerDomain>
    {
        private readonly int _npcId;
        public ItemAlreadyTaken(int npcId) { _npcId = npcId; }
        public bool Check(PlayerDomain player) => player.HasTakenItem(_npcId);
    }

    public class ItemNotYetTaken : ICondition<PlayerDomain>
    {
        private readonly int _npcId;
        public ItemNotYetTaken(int npcId) { _npcId = npcId; }
        public bool Check(PlayerDomain player) => !player.HasTakenItem(_npcId);
    }

    public class PokemonAlreadyTraded : ICondition<PlayerDomain>
    {
        private readonly int _pokedexId;
        public PokemonAlreadyTraded(int pokedexId) { _pokedexId = pokedexId; }
        public bool Check(PlayerDomain player) => player.HasTradedPokemon(_pokedexId);
    }

    // ── Composite conditions ──────────────────────────────────────────────────────

    public class AndCondition : ICondition<PlayerDomain>
    {
        private readonly ICondition<PlayerDomain>[] _conditions;
        public AndCondition(params ICondition<PlayerDomain>[] conditions) { _conditions = conditions; }
        public bool Check(PlayerDomain player) => _conditions.All(c => c.Check(player));
    }

    public class OrCondition : ICondition<PlayerDomain>
    {
        private readonly ICondition<PlayerDomain>[] _conditions;
        public OrCondition(params ICondition<PlayerDomain>[] conditions) { _conditions = conditions; }
        public bool Check(PlayerDomain player) => _conditions.Any(c => c.Check(player));
    }

    public class NotCondition : ICondition<PlayerDomain>
    {
        private readonly ICondition<PlayerDomain> _condition;
        public NotCondition(ICondition<PlayerDomain> condition) { _condition = condition; }
        public bool Check(PlayerDomain player) => !_condition.Check(player);
    }
    // ── Target Implementations ────────────────────────────────────────────────
    // ITarget interface lives in Interface/Move/IConditionAndTarget.cs.

    public class AttackerTarget : ITarget
    {
        public PokemonState Resolve(BattleState battle) => battle.Attacker;
    }

    public class DefenderTarget : ITarget
    {
        public PokemonState Resolve(BattleState battle) => battle.Defender;
    }

    // Always resolves to a specific pokemon regardless of attacker/defender roles.
    // Useful for field effects, weather damage, end-of-turn effects.
    public class SpecificTarget : ITarget
    {
        private readonly PokemonState _pokemon;
        public SpecificTarget(PokemonState pokemon) { _pokemon = pokemon; }
        public PokemonState Resolve(BattleState battle) => _pokemon;
    }
}
