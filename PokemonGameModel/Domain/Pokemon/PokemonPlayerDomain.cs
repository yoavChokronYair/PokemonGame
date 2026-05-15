using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Model.Helper.MathHelper;

namespace PokemonGame.Model.Domain.Pokemon
{
    public class PokemonPlayerDomain
    {
        // ─────────────────────────────────────────────────────────────────────
        // Core Identity
        // ─────────────────────────────────────────────────────────────────────

        public PokemonState PokemonState { get; set; } = null!;

        public int PokemonUID { get; set; }

        public string? Nickname { get; set; }

        public int OriginalTrainerID { get; set; }

        public string OriginalTrainerName { get; set; } = string.Empty;

        public GrowthRateType GrowthRate { get; set; }
            = GrowthRateType.MediumFast;

        // ─────────────────────────────────────────────────────────────────────
        // Catch / Obtain Metadata
        // ─────────────────────────────────────────────────────────────────────

        public ObtainMethodType ObtainMethod { get; set; }

        public string ObtainedAtRoute { get; set; } = string.Empty;

        public DateTime ObtainedAt { get; set; }

        public int ObtainedAtLevel { get; set; }

        public PokeBallType CaughtWithBall { get; set; }

        public string MetLocationText { get; set; } = string.Empty;

        // ─────────────────────────────────────────────────────────────────────
        // Experience
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// TOTAL accumulated experience.
        /// NOT "experience inside current level".
        /// </summary>
        public int Experience { get; set; }

        public int ExperienceToNextLevel
        {
            get
            {
                if (PokemonState.Level >= 100)
                    return 0;

                int next =
                    ExperienceHelper.GetTotalExpForLevel(
                        PokemonState.Level + 1,
                        GrowthRate);

                return next - Experience;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // EVs
        // ─────────────────────────────────────────────────────────────────────

        public int EV_HP { get; set; }

        public int EV_Attack { get; set; }

        public int EV_Defense { get; set; }

        public int EV_SpecialAttack { get; set; }

        public int EV_SpecialDefense { get; set; }

        public int EV_Speed { get; set; }

        public int TotalEVs =>
            EV_HP +
            EV_Attack +
            EV_Defense +
            EV_SpecialAttack +
            EV_SpecialDefense +
            EV_Speed;

        // ─────────────────────────────────────────────────────────────────────
        // IVs
        // ─────────────────────────────────────────────────────────────────────

        public int IV_HP { get; set; }

        public int IV_Attack { get; set; }

        public int IV_Defense { get; set; }

        public int IV_SpecialAttack { get; set; }

        public int IV_SpecialDefense { get; set; }

        public int IV_Speed { get; set; }

        // ─────────────────────────────────────────────────────────────────────
        // Friendship / Affection
        // ─────────────────────────────────────────────────────────────────────

        public int Friendship { get; set; }

        public int Affection { get; set; }

        // ─────────────────────────────────────────────────────────────────────
        // Status
        // ─────────────────────────────────────────────────────────────────────

        public StatusCondition PersistentStatus { get; set; }

        public int CurrentHP { get; set; }

        public bool IsFainted => CurrentHP <= 0;

        // ─────────────────────────────────────────────────────────────────────
        // Moves
        // ─────────────────────────────────────────────────────────────────────

        public MoveState?[] Moves { get; set; }
            = new MoveState?[4];

        // ─────────────────────────────────────────────────────────────────────
        // Battle State
        // ─────────────────────────────────────────────────────────────────────

        public int[] StatStages { get; private set; }
            = new int[7];

        public List<object> VolatileStatuses { get; set; }
            = new();

        public int LastDamageDealt { get; set; }

        public int LastDamageTaken { get; set; }

        public int turnsActive { get; set; }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        public int PokedexId => PokemonState.PokedexId;

        // ─────────────────────────────────────────────────────────────────────
        // Constructors
        // ─────────────────────────────────────────────────────────────────────

        public PokemonPlayerDomain()
        {
        }

        public PokemonPlayerDomain(
            WildPokemonDomain wild,
            ObtainMethodType obtainMethod,
            string obtainedAtRoute,
            PokeBallType caughtWithBall = PokeBallType.PokeBall,
            string? nickname = null)
        {
            PokemonState = wild.pokemonState;

            ObtainMethod = obtainMethod;

            ObtainedAtRoute = obtainedAtRoute;

            CaughtWithBall = caughtWithBall;

            ObtainedAt = DateTime.Now;

            ObtainedAtLevel = wild.pokemonState.Level;

            MetLocationText = obtainedAtRoute;

            GrowthRate = wild.GrowthRate;

            Nickname = nickname;

            OriginalTrainerID =
                PlayerDomain.Instance.trainerInfo.TrainerID;

            OriginalTrainerName =
                PlayerDomain.Instance.trainerInfo.Name;

            CurrentHP = wild.pokemonState.MaxHP;

            Friendship = wild.BaseFriendshipYield;

            Experience =
                ExperienceHelper.GetTotalExpForLevel(
                    wild.pokemonState.Level,
                    GrowthRate);

            if (wild.pokemonState.Moves != null)
            {
                for (int i = 0;
                     i < Math.Min(wild.pokemonState.Moves.Count, 4);
                     i++)
                {
                    Moves[i] =
                        ((MoveState)wild.pokemonState.Moves[i])
                        .Clone();
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Factory
        // ─────────────────────────────────────────────────────────────────────

        public static PokemonPlayerDomain FromWildCatch(
            WildPokemonDomain wild,
            string caughtOnRoute,
            PokeBallType ballUsed,
            string? nickname = null)
        {
            var state = wild.pokemonState;

            return new PokemonPlayerDomain(
                wild,
                ObtainMethodType.Caught,
                caughtOnRoute,
                ballUsed,
                nickname)
            {
                Friendship = wild.BaseFriendshipYield,

                IV_HP = state.IVs[0],
                IV_Attack = state.IVs[1],
                IV_Defense = state.IVs[2],
                IV_SpecialAttack = state.IVs[3],
                IV_SpecialDefense = state.IVs[4],
                IV_Speed = state.IVs[5],

                EV_HP = 0,
                EV_Attack = 0,
                EV_Defense = 0,
                EV_SpecialAttack = 0,
                EV_SpecialDefense = 0,
                EV_Speed = 0,
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // Status
        // ─────────────────────────────────────────────────────────────────────

        public void ClearStatus()
        {
            PersistentStatus = StatusCondition.None;
        }

        public void ResetStatStages()
        {
            for (int i = 0; i < StatStages.Length; i++)
            {
                StatStages[i] = 0;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // EXP
        // ─────────────────────────────────────────────────────────────────────

        public LevelUpResult GainExperience(int amount)
        {
            LevelUpResult result = new();

            if (PokemonState.Level >= 100)
                return result;

            Experience += amount;

            while (CanLevelUp())
            {
                LevelUp(result);
            }

            return result;
        }

        private bool CanLevelUp()
        {
            if (PokemonState.Level >= 100)
                return false;

            int required =
                ExperienceHelper.GetTotalExpForLevel(
                    PokemonState.Level + 1,
                    GrowthRate);

            return Experience >= required;
        }

        private void LevelUp(LevelUpResult result)
        {
            int oldMaxHp = PokemonState.MaxHP;

            PokemonState.Level++;

            result.GainedLevels.Add(PokemonState.Level);

            RecalculateStats(oldMaxHp);

            LearnMovesForCurrentLevel(result);

            CheckEvolution(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Stats
        // ─────────────────────────────────────────────────────────────────────

        private void RecalculateStats(int oldMaxHp)
        {
            var natureModifiers =
                NatureConstants.GetNatureModifiers(
                    PokemonState.Nature);

            PokemonState.MaxHP =
                PokemonStatCalculatorHelper.CalculateHP(
                    PokemonState.Base.HP,
                    IV_HP,
                    EV_HP,
                    PokemonState.Level);

            PokemonState.BaseAttack =
                PokemonStatCalculatorHelper.CalculateStat(
                    PokemonState.Base.Attack,
                    IV_Attack,
                    EV_Attack,
                    PokemonState.Level,
                    natureModifiers.atk);

            PokemonState.BaseDefense =
                PokemonStatCalculatorHelper.CalculateStat(
                    PokemonState.Base.Defense,
                    IV_Defense,
                    EV_Defense,
                    PokemonState.Level,
                    natureModifiers.def);

            PokemonState.BaseSpecialAttack =
                PokemonStatCalculatorHelper.CalculateStat(
                    PokemonState.Base.SpecialAttack,
                    IV_SpecialAttack,
                    EV_SpecialAttack,
                    PokemonState.Level,
                    natureModifiers.spAtk);

            PokemonState.BaseSpecialDefense =
                PokemonStatCalculatorHelper.CalculateStat(
                    PokemonState.Base.SpecialDefense,
                    IV_SpecialDefense,
                    EV_SpecialDefense,
                    PokemonState.Level,
                    natureModifiers.spDef);

            PokemonState.BaseSpeed =
                PokemonStatCalculatorHelper.CalculateStat(
                    PokemonState.Base.Speed,
                    IV_Speed,
                    EV_Speed,
                    PokemonState.Level,
                    natureModifiers.speed);

            int hpGain =
                PokemonState.MaxHP - oldMaxHp;

            CurrentHP += hpGain;

            if (CurrentHP > PokemonState.MaxHP)
                CurrentHP = PokemonState.MaxHP;

            PokemonState.CurrentHP = CurrentHP;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Moves
        // ─────────────────────────────────────────────────────────────────────

        private void LearnMovesForCurrentLevel(
            LevelUpResult result)
        {
            if (PokemonState.Learnset == null)
                return;

            var learnableMoves =
                PokemonState.Learnset
                    .Where(x => x.Level == PokemonState.Level);

            foreach (var entry in learnableMoves)
            {
                TryLearnMove(entry.Move, result);
            }
        }

        private void TryLearnMove(
            MoveState move,
            LevelUpResult result)
        {
            bool alreadyKnows =
                Moves.Any(x => x?.Name == move.Name);

            if (alreadyKnows)
                return;

            int emptySlot =
                Array.FindIndex(Moves, x => x == null);

            // Free slot
            if (emptySlot != -1)
            {
                Moves[emptySlot] = move.Clone();

                PokemonState.Moves.Add(
                    move.Clone());

                result.LearnedMoves.Add(
                    new MoveLearnResult
                    {
                        Level = PokemonState.Level,
                        Move = move,
                        NeedsReplacement = false
                    });

                return;
            }

            // Need replacement
            result.LearnedMoves.Add(
                new MoveLearnResult
                {
                    Level = PokemonState.Level,
                    Move = move,
                    NeedsReplacement = true
                });
        }

        public void ReplaceMove(
            int slot,
            MoveState newMove)
        {
            if (slot < 0 || slot >= 4)
                return;

            Moves[slot] = newMove.Clone();

            if (slot < PokemonState.Moves.Count)
            {
                PokemonState.Moves[slot] =
                    newMove.Clone();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Evolution
        // ─────────────────────────────────────────────────────────────────────

        private void CheckEvolution(LevelUpResult result)
        {
            if (PokemonState.Evolution == null)
                return;

            if (PokemonState.Level <
                PokemonState.Evolution.LevelRequired)
                return;

            result.Evolved = true;

            result.EvolutionTarget =
                PokemonState.Evolution.ToPokemonId;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Rare Candy
        // ─────────────────────────────────────────────────────────────────────

        public LevelUpResult UseRareCandy()
        {
            if (PokemonState.Level >= 100)
                return new();

            int required =
                ExperienceHelper.GetTotalExpForLevel(
                    PokemonState.Level + 1,
                    GrowthRate);

            Experience = required;

            return GainExperience(0);
        }

        // ─────────────────────────────────────────────────────────────────────
        // EVs
        // ─────────────────────────────────────────────────────────────────────

        public void GainEVs(BaseStats evYield)
        {
            EV_HP += evYield.HP;
            EV_Attack += evYield.Attack;
            EV_Defense += evYield.Defense;
            EV_SpecialAttack += evYield.SpecialAttack;
            EV_SpecialDefense += evYield.SpecialDefense;
            EV_Speed += evYield.Speed;

            NormalizeEVs();
        }

        public void NormalizeEVs()
        {
            EV_HP = MathHelper.Clamp(EV_HP, 0, 252);
            EV_Attack = MathHelper.Clamp(EV_Attack, 0, 252);
            EV_Defense = MathHelper.Clamp(EV_Defense, 0, 252);
            EV_SpecialAttack = MathHelper.Clamp(EV_SpecialAttack, 0, 252);
            EV_SpecialDefense = MathHelper.Clamp(EV_SpecialDefense, 0, 252);
            EV_Speed = MathHelper.Clamp(EV_Speed, 0, 252);

            int total =
                EV_HP +
                EV_Attack +
                EV_Defense +
                EV_SpecialAttack +
                EV_SpecialDefense +
                EV_Speed;

            if (total <= 510)
                return;

            double scale = 510.0 / total;

            EV_HP = (int)Math.Floor(EV_HP * scale);
            EV_Attack = (int)Math.Floor(EV_Attack * scale);
            EV_Defense = (int)Math.Floor(EV_Defense * scale);
            EV_SpecialAttack = (int)Math.Floor(EV_SpecialAttack * scale);
            EV_SpecialDefense = (int)Math.Floor(EV_SpecialDefense * scale);
            EV_Speed = (int)Math.Floor(EV_Speed * scale);
        }
    }

   
}