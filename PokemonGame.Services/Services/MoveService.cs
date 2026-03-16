using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.Repositories.SQLite;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    // Assembles a fully hydrated MoveTree from flat DB rows.
    // Call GetMove("Flamethrower") and get the entire tree back —
    // no dangling IDs, every child already resolved.
    public interface IMoveService
    {
        MoveTree? GetMove(string name);
    }

    public class MoveService : IMoveService
    {
        private readonly SQLiteMoveRepository _repo;

        // Visited sets prevent infinite loops in self-referencing trees
        // (e.g. Charge → ReleaseAttempt → Charge)
        private readonly HashSet<int> _visitedEffects = new();
        private readonly HashSet<int> _visitedNumbers = new();
        private readonly HashSet<int> _visitedAttempts = new();

        public MoveService()
        {
            _repo = ServiceFactory.Instance.MoveRepository;
        }

        // ── Public entry point ───────────────────────────────────────────────────

        public MoveTree? GetMove(string name)
        {
            var move = _repo.LoadMoveData(name);
            if (move == null)
            {
                return null;
            }

            // Reset visited sets per call so each tree build is independent
            _visitedEffects.Clear();
            _visitedNumbers.Clear();
            _visitedAttempts.Clear();

            var attempts = _repo.LoadAttemptsForMove(move.Id);
            var tree = new MoveTree
            {
                Move = move,
                Priority = move.Priority,
                CritStage = move.CritStage,
                Description = move.Description,
            };

            foreach (var attempt in attempts)
            {
                tree.Attempts.Add(BuildAttempt(attempt));
            }

            return tree;
        }

        // ── Attempt builder ──────────────────────────────────────────────────────

        private MoveAttempt BuildAttempt(AttemptRow row)
        {
            if (_visitedAttempts.Contains(row.Id))
            {
                return new MoveAttempt { Id = row.Id, Type = row.Type }; // break cycle
            }

            _visitedAttempts.Add(row.Id);

            var attempt = new MoveAttempt
            {
                Id = row.Id,
                Type = row.Type,
                AccuracyValue = row.AccuracyValue,
                StopOnMiss = row.StopOnMiss == 1,
                RampageMinTurns = row.RampageMinTurns,
                RampageMaxTurns = row.RampageMaxTurns,
            };

            // Attempt
            if (row.OnHitEffectId.HasValue)
            {
                attempt.OnHit = BuildEffect(row.OnHitEffectId.Value);
            }

            if (row.OnMissEffectId.HasValue)
            {
                attempt.OnMiss = BuildEffect(row.OnMissEffectId.Value);
            }

            if (row.AfterEffectId.HasValue)
            {
                attempt.After = BuildEffect(row.AfterEffectId.Value);
            }

            // Cascade
            foreach (var step in _repo.LoadCascadeSteps(row.Id))
            {
                var child = _repo.LoadAttempt(step.ChildAttemptId);
                if (child != null)
                {
                    attempt.CascadeSteps.Add(BuildAttempt(child));
                }
            }

            // Combo
            if (row.HitsNumberId.HasValue)
            {
                attempt.HitsNumber = BuildNumber(row.HitsNumberId.Value);
            }

            // Charge
            if (row.ChargeEffectId.HasValue)
            {
                attempt.ChargeEffect = BuildEffect(row.ChargeEffectId.Value);
            }

            if (row.ReleaseAttemptId.HasValue)
            {
                var release = _repo.LoadAttempt(row.ReleaseAttemptId.Value);
                if (release != null)
                {
                    attempt.ReleaseAttempt = BuildAttempt(release);
                }
            }

            // Rampage
            if (row.AfterRampageEffectId.HasValue)
            {
                attempt.AfterRampage = BuildEffect(row.AfterRampageEffectId.Value);
            }

            return attempt;
        }

        // ── Effect builder ───────────────────────────────────────────────────────

        private MoveEffect? BuildEffect(int id)
        {
            if (_visitedEffects.Contains(id))
            {
                return null; // break cycle
            }

            _visitedEffects.Add(id);

            var row = _repo.LoadEffect(id);
            if (row == null)
            {
                return null;
            }

            var effect = new MoveEffect
            {
                Id = row.Id,
                Type = row.Type,
                Target = row.Target,
                HealTarget = row.HealTarget,
                ChanceProbability = row.ChanceProbability,
                Stat = row.Stat,
                StatStages = row.StatStages,
                SleepMinTurns = row.SleepMinTurns,
                SleepMaxTurns = row.SleepMaxTurns,
                ConfuseMinTurns = row.ConfuseMinTurns,
                ConfuseMaxTurns = row.ConfuseMaxTurns,
                IsToxic = row.IsToxic == 1,
                Weather = row.Weather,
                WeatherTurns = row.WeatherTurns,
                Screen = row.Screen,
                ScreenTurns = row.ScreenTurns,
                BattleSide = row.BattleSide,
                Hazard = row.Hazard,
                ChargeTurns = row.ChargeTurns,
            };

            // Number (damage/healing formula)
            if (row.NumberId.HasValue)
            {
                effect.Number = BuildNumber(row.NumberId.Value);
            }

            // Chance → child effect
            if (row.ChildEffectId.HasValue)
            {
                effect.ChanceChild = BuildEffect(row.ChildEffectId.Value);
            }

            // Conditional
            if (row.ConditionId.HasValue)
            {
                effect.Condition = BuildCondition(row.ConditionId.Value);
            }

            if (row.OnPassEffectId.HasValue)
            {
                effect.OnPass = BuildEffect(row.OnPassEffectId.Value);
            }

            if (row.OnFailEffectId.HasValue)
            {
                effect.OnFail = BuildEffect(row.OnFailEffectId.Value);
            }

            // MultiStatChange rows
            effect.StatChanges = _repo.LoadMultiStatChanges(id);

            // Sequence → ordered child effects
            foreach (var step in _repo.LoadSequenceSteps(id))
            {
                var child = BuildEffect(step.ChildEffectId);
                if (child != null)
                {
                    effect.SequenceSteps.Add(child);
                }
            }

            _visitedEffects.Remove(id); // allow same effect to appear in separate branches
            return effect;
        }

        // ── Number builder ───────────────────────────────────────────────────────

        private MoveNumber? BuildNumber(int id)
        {
            if (_visitedNumbers.Contains(id))
            {
                return null; // break cycle
            }

            _visitedNumbers.Add(id);

            var row = _repo.LoadNumber(id);
            if (row == null)
            {
                return null;
            }

            var number = new MoveNumber
            {
                Id = row.Id,
                Type = row.Type,
                ExactValue = row.ExactValue,
                RangeMin = row.RangeMin,
                RangeMax = row.RangeMax,
                Target = row.Target,
            };

            // Weighted entries
            if (row.Type == "Weighted")
            {
                number.WeightedEntries = _repo.LoadWeightedEntries(id);
            }

            // Recursive left/right (Product / Sum / Quotient)
            if (row.LeftNumberId.HasValue)
            {
                number.Left = BuildNumber(row.LeftNumberId.Value);
            }

            if (row.RightNumberId.HasValue)
            {
                number.Right = BuildNumber(row.RightNumberId.Value);
            }

            _visitedNumbers.Remove(id);
            return number;
        }

        // ── Condition builder ────────────────────────────────────────────────────

        private MoveCondition? BuildCondition(int id)
        {
            var row = _repo.LoadCondition(id);
            if (row == null)
            {
                return null;
            }

            var condition = new MoveCondition
            {
                Id = row.Id,
                Type = row.Type,
                Probability = row.Probability,
                Weather = row.Weather,
                Status = row.Status,
                VolatileStatus = row.VolatileStatus,
                HpFraction = row.HpFraction,
                PokemonType = row.PokemonType,
            };

            if (row.LeftConditionId.HasValue)
            {
                condition.Left = BuildCondition(row.LeftConditionId.Value);
            }

            if (row.RightConditionId.HasValue)
            {
                condition.Right = BuildCondition(row.RightConditionId.Value);
            }

            if (row.InnerConditionId.HasValue)
            {
                condition.Inner = BuildCondition(row.InnerConditionId.Value);
            }

            return condition;

        }
    }
}