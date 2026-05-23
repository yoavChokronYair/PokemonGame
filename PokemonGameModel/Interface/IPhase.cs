using System.Xml.Linq;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Model.DesignPatterns;

namespace PokemonGame.Model.Interface
{
    public interface IPhase
    {
        void Run(BattleState battleState);
    }
    // ── 1. Start Battle ──────────────────────────────────────────────
    // Triggered when the BattleManager is first initialized
    public class StartBattle : IPhase
    {
        public void Run(BattleState state)
        {
            state.Logger.LogSetup("A battle has Started!");
            // Trigger switch-in effects for the starting two
            new SwitchIn(state.Attacker, state.AttackerSide).Run(state);
            new SwitchIn(state.Defender, state.DefenderSide).Run(state);
           
        }
    }

    public class SwitchIn : IPhase
    {
        private readonly PokemonState _incoming;
        private readonly BattleSideState _sideState;

        public SwitchIn(PokemonState incoming, BattleSideState sideState)
        {
            _incoming = incoming;
            _sideState = sideState;
        }

        public void Run(BattleState state)
        {
            state.Logger.LogSwitch($"{_incoming.Name} entered the battle!");

            ApplyEntryHazards(state);

            if (_incoming.IsFainted)
                return;

            if (_incoming.Ability is OnSwitchIn switchAbility)
            {
                switchAbility.Apply(state);
            }
        }

        private void ApplyEntryHazards(BattleState state)
        {
            ApplyStealthRock(state);
            ApplySpikes(state);
            ApplyToxicSpikes(state);
            ApplyStickyWeb(state);
        }

        private void ApplyStealthRock(BattleState state)
        {
            int layers = _sideState.GetHazardLayers(Hazard.StealthRock);

            if (layers <= 0)
                return;

            int damage = Math.Max(1, _incoming.MaxHP / 8);

            _incoming.TakeDamage(damage);

            state.Logger.LogSwitch($"Pointed stones dug into {_incoming.Name}!");
        }

        private void ApplySpikes(BattleState state)
        {
            int layers = _sideState.GetHazardLayers(Hazard.Spikes);

            if (layers <= 0)
                return;

            if (IsGroundImmune())
                return;

            int damage = layers switch
            {
                1 => Math.Max(1, _incoming.MaxHP / 8),
                2 => Math.Max(1, _incoming.MaxHP / 6),
                _ => Math.Max(1, _incoming.MaxHP / 4)
            };

            _incoming.TakeDamage(damage);

            state.Logger.LogSwitch($"{_incoming.Name} was hurt by Spikes!");
        }

        private void ApplyToxicSpikes(BattleState state)
        {
            int layers = _sideState.GetHazardLayers(Hazard.ToxicSpikes);

            if (layers <= 0)
                return;

            if (IsGroundImmune())
                return;

            if (_incoming.HasType(PokemonType.Poison))
            {
                _sideState.RemoveHazard(Hazard.ToxicSpikes);

                state.Logger.LogSwitch($"{_incoming.Name} absorbed the Toxic Spikes!");
                return;
            }

            if (_incoming.HasType(PokemonType.Steel))
                return;

            if (layers == 1)
            {
                _incoming.ApplyStatus(StatusCondition.Poison);

                state.Logger.LogStatus(
                    $"{_incoming.Name} was poisoned by Toxic Spikes!");
            }
            else
            {
                // If you do not have BadlyPoisoned yet, keep Poison here.
                _incoming.ApplyStatus(StatusCondition.Poison);

                state.Logger.LogStatus(
                    $"{_incoming.Name} was badly poisoned by Toxic Spikes!");
            }
        }

        private void ApplyStickyWeb(BattleState state)
        {
            int layers = _sideState.GetHazardLayers(Hazard.StickyWeb);

            if (layers <= 0)
                return;

            if (IsGroundImmune())
                return;

            _incoming.ChangeStatStage(Stat.Speed, -1);

            state.Logger.LogSwitch($"{_incoming.Name}'s Speed fell because of Sticky Web!");
        }

        private bool IsGroundImmune()
        {
            return _incoming.HasType(PokemonType.Flying)
                   || IsAbility("Levitate");
        }

        private bool IsAbility(string abilityName)
        {
            return _incoming.Ability is AbilityState ability &&
                   ability.Name == abilityName;
        }
    }


    // ── 3. Begin Turn ────────────────────────────────────────────────
    public class BeginTurn : IPhase
    {
        public void Run(BattleState state)
        {
            state.IncrementTurn();
            state.ResetDamage();
            state.Logger.LogTurnStart($"--- Turn {state.TurnNumber} ---");
            state.Logger.LogTurnStart($"What will {state.Attacker.Name} do?");

            ApplyIfTurnStart(state.Attacker.Ability, state);
            ApplyIfTurnStart(state.Defender.Ability, state);
        }

        private void ApplyIfTurnStart(IAbility? ability, BattleState state)
        {
            if (ability is OnTurnStart turnStartAbility)
                turnStartAbility.Apply(state);
        }
    }

   

    // ── 4. Move Execution ────────────────────────────────────────────
    public class MoveExecution : IPhase
    {
        private readonly IMove? _move;
        private readonly PokemonState _user;
        private readonly PokemonState _target;

        public MoveExecution(IMove? move, PokemonState user, PokemonState target)
        {
            _move = move;
            _user = user;
            _target = target;
        }

        public void Run(BattleState state)
        {
            if (_user.IsFainted || _move == null || _user.Status == StatusCondition.Sleep || _user.Status == StatusCondition.Freeze) return;
            if ((_user.Status == StatusCondition.Paralysis && RandomHelper.Next(0, 4) < 1))
            {
                state.Logger.Log($"{_user.Name} is paralyzed and can't move!");
                return;
            }

            // Use the new method we just discussed
            state.UpdateActivePair(_user, _target);
            _move.Execute(state);
            // ── 5. Getting Hit (The "OnHit" Phase) ──
            // After damage, check for Static, Flame Body, etc.
            new ResolveOnHitEffect(_user, _target).Run(state);
        }
    }

    // ── 5. Resolve OnHit ─────────────────────────────────────────────
    public class ResolveOnHitEffect : IPhase
    {
        private readonly PokemonState _attacker;
        private readonly PokemonState _defender;

        public ResolveOnHitEffect(PokemonState attacker, PokemonState defender)
        {
            _attacker = attacker;
            _defender = defender;
        }

        public void Run(BattleState state)
        {
            // Defender triggers (e.g. Static)
            if (_defender.Ability is OnHit defAbility)
                defAbility.Apply(state);

            // Attacker triggers (e.g. Poison Touch)
            if (_attacker.Ability is OnHit atkAbility)
                atkAbility.Apply(state);
        }
    }

    // ── 6. End Turn ──────────────────────────────────────────────────
    public class EndTurn : IPhase
    {
        public void Run(BattleState state)
        {
            state.WeatherService.TickWeather();
            state.TerrainService.TickTerrain();
            state.Field.Tick();
            state.AttackerSide.Tick();
            state.DefenderSide.Tick();
            state.StatusService.ApplyEndOfTurnStatus(state.Attacker);
            state.StatusService.ApplyEndOfTurnStatus(state.Defender);
            state.Attacker.turnsActive++;
            state.Defender.turnsActive++;
        }
    }
    // ── 7. Resolve Turn ──────────────────────────────────────────────────
    public class ResolveTurn : IPhase
    {
        private readonly IMove? _playerMove;
        private readonly IMove? _botMove;
        private readonly PokemonState _player;
        private readonly PokemonState _bot;

        public ResolveTurn(IMove? playerMove, IMove? botMove, PokemonState player, PokemonState bot)
        {
            _playerMove = playerMove;
            _botMove = botMove;
            _player = player;
            _bot = bot;
        }

        public void Run(BattleState state)
        {
            int playerPriority = (_playerMove as MoveState)?.Priority ?? 0;
            int botPriority = (_botMove as MoveState)?.Priority ?? 0;

            bool playerFirst = state.AttackerMovesFirst(playerPriority, botPriority);

            if (playerFirst)
            {
                new MoveExecution(_playerMove, _player, _bot).Run(state);
                if (!_bot.IsFainted)
                    new MoveExecution(_botMove, _bot, _player).Run(state);
            }
            else
            {
                new MoveExecution(_botMove, _bot, _player).Run(state);
                if (!_player.IsFainted)
                    new MoveExecution(_playerMove, _player, _bot).Run(state);
            }
        }
    }

    // ── 8. Handle Faints ─────────────────────────────────────────────────
    public class HandleFaints : IPhase
    {
        private readonly PokemonState _player;
        private readonly PokemonState _bot;

        public HandleFaints(PokemonState player, PokemonState bot)
        {
            _player = player;
            _bot = bot;
        }

        public void Run(BattleState state)
        {
            if (_player.IsFainted)
                state.Logger.Log($"{_player.Name} fainted!");

            if (_bot.IsFainted)
                state.Logger.Log($"{_bot.Name} fainted!");
        }
    }
}
