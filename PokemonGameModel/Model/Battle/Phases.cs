using System;
using System.Collections.Generic;
using System.Text;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.DesignPatterns;

namespace PokemonGame.Model.Model.Battle
{
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
        private readonly IMove? _selectedMove;
        private readonly PokemonState _user;
        private readonly PokemonState _target;

        public MoveExecution(IMove? move, PokemonState user, PokemonState target)
        {
            _selectedMove = move;
            _user = user;
            _target = target;
        }

        public void Run(BattleState state)
        {
            if (_user.IsFainted)
                return;

            IMove? move = ResolveChoiceLockedMove(state);

            if (move == null)
                return;

            if (!CanActThisTurn(state))
                return;

            state.UpdateActivePair(_user, _target);

            if (!CanTarget(move, state))
                return;

            move.Execute(state);

            RegisterChoiceLockIfNeeded(move);

            if (!_target.IsFainted)
                new ResolveOnHitEffect(_user, _target).Run(state);
        }

        private bool CanActThisTurn(BattleState state)
        {
            if (!CanMoveBecauseOfMajorStatus(state))
                return false;

            if (!CanMoveBecauseOfFlinch(state))
                return false;

            if (!CanMoveBecauseOfTruant(state))
                return false;

            if (!CanMoveBecauseOfConfusion(state))
                return false;

            return true;
        }

        private bool CanMoveBecauseOfMajorStatus(BattleState state)
        {
            switch (_user.Status)
            {
                case StatusCondition.Sleep:
                    state.Logger.LogStatus($"{_user.Name} is fast asleep!");
                    return false;

                case StatusCondition.Freeze:
                    state.Logger.LogStatus($"{_user.Name} is frozen solid!");
                    return false;

                case StatusCondition.Paralysis:
                    if (RandomHelper.Next(0, 4) == 0)
                    {
                        state.Logger.LogStatus($"{_user.Name} is paralyzed and can't move!");
                        return false;
                    }

                    return true;

                default:
                    return true;
            }
        }

        private bool CanMoveBecauseOfFlinch(BattleState state)
        {
            if (!_user.HasVolatileStatus(VolatileStatus.Flinch))
                return true;

            _user.RemoveVolatileStatus(VolatileStatus.Flinch);
            state.Logger.LogStatus($"{_user.Name} flinched and couldn't move!");

            return false;
        }

        private bool CanMoveBecauseOfTruant(BattleState state)
        {
            if (!HasAbility(_user, "Truant"))
                return true;

            // turnsActive starts at 0 on switch-in.
            // Turn 0: can move.
            // Turn 1: loafs around.
            // Turn 2: can move.
            if (_user.turnsActive % 2 == 1)
            {
                state.Logger.LogStatus($"{_user.Name} is loafing around!");
                return false;
            }

            return true;
        }

        private bool CanMoveBecauseOfConfusion(BattleState state)
        {
            if (!_user.HasVolatileStatus(VolatileStatus.Confusion))
                return true;

            if (!_user.VolatileStatuses.TryGetValue(VolatileStatus.Confusion, out int turnsLeft))
            {
                _user.RemoveVolatileStatus(VolatileStatus.Confusion);
                return true;
            }

            if (turnsLeft <= 0)
            {
                _user.RemoveVolatileStatus(VolatileStatus.Confusion);
                state.Logger.LogStatus($"{_user.Name} snapped out of confusion!");
                return true;
            }

            state.Logger.LogStatus($"{_user.Name} is confused!");

            _user.VolatileStatuses[VolatileStatus.Confusion] = turnsLeft - 1;

            // Gen III confusion self-hit chance is 50%.
            if (RandomHelper.Next(0, 2) == 0)
                return true;

            int damage = Math.Max(1, _user.MaxHP / 8);

            _user.TakeDamage(damage);

            state.Logger.LogStatus($"{_user.Name} hurt itself in its confusion!");

            return false;
        }

        private IMove? ResolveChoiceLockedMove(BattleState state)
        {
            IMove? lockedMove = _user.GetLockedMove();

            if (lockedMove == null)
                return _selectedMove;

            if (_selectedMove == null)
                return lockedMove;

            if (ReferenceEquals(_selectedMove, lockedMove))
                return lockedMove;

            string lockedName = (lockedMove as MoveState)?.Name ?? "its locked move";

            state.Logger.LogStatus($"{_user.Name} is locked into {lockedName}!");

            return lockedMove;
        }

        private void RegisterChoiceLockIfNeeded(IMove move)
        {
            if (!IsChoiceLockedByItem(_user))
                return;

            if (_user.GetLockedMove() != null)
                return;

            _user.CopyMove(move);
            _user.LockToLastMove();
        }

        private static bool IsChoiceLockedByItem(PokemonState pokemon)
        {
            if (pokemon.HeldItem == null)
                return false;

            string itemName = pokemon.HeldItem.ToString() ?? string.Empty;

            return itemName.IndexOf("Choice Band", StringComparison.OrdinalIgnoreCase) >= 0
                   || itemName.IndexOf("Choice Scarf", StringComparison.OrdinalIgnoreCase) >= 0
                   || itemName.IndexOf("Choice Specs", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool HasAbility(PokemonState pokemon, string abilityName)
        {
            return pokemon.Ability is AbilityState ability &&
                   ability.Name.Equals(abilityName, StringComparison.OrdinalIgnoreCase);
        }

        private bool CanTarget(IMove move, BattleState state)
        {
            if (move is not MoveState moveState)
                return true;

            switch (moveState.Target)
            {
                case MoveTarget.Self:
                    return !_user.IsFainted;

                case MoveTarget.Opponent:
                    if (_target.IsFainted)
                    {
                        state.Logger.Log($"{_user.Name}'s move had no target!");
                        return false;
                    }

                    return true;

                case MoveTarget.Both:
                    return !_user.IsFainted || !_target.IsFainted;

                case MoveTarget.AllOpponents:
                    if (_target.IsFainted)
                    {
                        state.Logger.Log($"{_user.Name}'s move had no opposing target!");
                        return false;
                    }

                    return true;

                case MoveTarget.AllAllies:
                    return !_user.IsFainted;

                default:
                    return true;
            }
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
            state.StatusService.ApplyEndOfTurnStatus(state, state.Attacker);
            state.StatusService.ApplyEndOfTurnStatus(state, state.Defender);
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
