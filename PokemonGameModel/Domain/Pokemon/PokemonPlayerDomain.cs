using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Player;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Helper.MathHelper;

namespace PokemonGame.Model.Domain.Pokemon
{
    public class PokemonPlayerDomain
    {
        // ─── Core Identity ────────────────────────────────────────────────────

        public PokemonState PokemonState { get; set; }
        public int PokemonUID { get; set; }

        /// <summary>Nickname given by the player. Null means no nickname set.</summary>
        public string? Nickname { get; set; }

        /// <summary>Trainer ID of the OT (Original Trainer)</summary>
        public int OriginalTrainerID { get; set; }
        public GrowthRateType GrowthRate { get; set; }

        /// <summary>Name of the Original Trainer</summary>
        public string OriginalTrainerName { get; set; }

        // ─── Catch / Obtain Metadata ──────────────────────────────────────────

        public ObtainMethodType ObtainMethod { get; set; }
        public string ObtainedAtRoute { get; set; }
        public DateTime ObtainedAt { get; set; }
        public int ObtainedAtLevel { get; set; }
        public PokeBallType CaughtWithBall { get; set; }
        public string MetLocationText { get; set; }

        // ─── Experience & Levelling ───────────────────────────────────────────

        public int Experience { get; set; }

        public int ExperienceToNextLevel =>
            ExperienceHelper.GetExpToNextLevel(PokemonState.Level, GrowthRate);

        // ─── Effort Values (EVs) ─────────────────────────────────────────────

        public int EV_HP { get; set; }
        public int EV_Attack { get; set; }
        public int EV_Defense { get; set; }
        public int EV_SpecialAttack { get; set; }
        public int EV_SpecialDefense { get; set; }
        public int EV_Speed { get; set; }

        public int TotalEVs => EV_HP + EV_Attack + EV_Defense + EV_SpecialAttack + EV_SpecialDefense + EV_Speed;

        // ─── Individual Values (IVs) ──────────────────────────────────────────

        public int IV_HP { get; set; }
        public int IV_Attack { get; set; }
        public int IV_Defense { get; set; }
        public int IV_SpecialAttack { get; set; }
        public int IV_SpecialDefense { get; set; }
        public int IV_Speed { get; set; }

        // ─── Friendship & Affection ───────────────────────────────────────────

        public int Friendship { get; set; }
        public int Affection { get; set; }

        // ─── Status ───────────────────────────────────────────────────────────

        public StatusCondition PersistentStatus { get; set; }
        public int CurrentHP { get; set; }
        public bool IsFainted => CurrentHP <= 0;

        // ─── Moves ────────────────────────────────────────────────────────────

        public MoveState?[] Moves { get; set; } = new MoveState?[4];

        // ─── Constructor ─────────────────────────────────────────────────────

        /// <summary>
        /// Base constructor. All factory methods route through here.
        /// <paramref name="nickname"/> is null when the player hasn't named the Pokémon.
        /// </summary>
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

            // Nickname: use what was passed in, or null (display will fall back to species name)
            Nickname = nickname;

            // Pull OT from the live player singleton
            OriginalTrainerID = PlayerDomain.Instance.trainerInfo.TrainerID;
            OriginalTrainerName = PlayerDomain.Instance.trainerInfo.Name;

            // Start HP at full
            CurrentHP = wild.pokemonState.MaxHP;

            // Carry over base friendship defined on the species
            Friendship = wild.BaseFriendshipYield;

            // Copy moves from the state into slots
            if (wild.pokemonState.Moves != null)
            {
                for (int i = 0; i < Math.Min(wild.pokemonState.Moves.Count, 4); i++)
                    Moves[i] = wild.pokemonState.Moves[i] as MoveState;
            }
        }

        // ─── Factory Methods ──────────────────────────────────────────────────

        /// <summary>
        /// Creates a <see cref="PokemonPlayerDomain"/> from a freshly caught wild Pokémon.
        /// Pass a nickname if the player named it on the catch screen; leave null otherwise.
        /// </summary>
        public static PokemonPlayerDomain FromWildCatch(
            WildPokemonDomain wild,
            string caughtOnRoute,
            PokeBallType ballUsed,
            string? nickname = null)
        {
            var state = wild.pokemonState;

            var playerPokemon = new PokemonPlayerDomain(
                wild,
                ObtainMethodType.Caught,
                caughtOnRoute,
                ballUsed,
                nickname)   // ← nickname flows through to the base constructor
            {
                Friendship = wild.BaseFriendshipYield,

                // ── IVs (from state array if present, otherwise generate) ──────
                IV_HP = state.IVs is { Length: >= 6 } ? state.IVs[0] : RNGHelper.GenerateIV(),
                IV_Attack = state.IVs is { Length: >= 6 } ? state.IVs[1] : RNGHelper.GenerateIV(),
                IV_Defense = state.IVs is { Length: >= 6 } ? state.IVs[2] : RNGHelper.GenerateIV(),
                IV_SpecialAttack = state.IVs is { Length: >= 6 } ? state.IVs[3] : RNGHelper.GenerateIV(),
                IV_SpecialDefense = state.IVs is { Length: >= 6 } ? state.IVs[4] : RNGHelper.GenerateIV(),
                IV_Speed = state.IVs is { Length: >= 6 } ? state.IVs[5] : RNGHelper.GenerateIV(),

                // Wild Pokémon always start with zero EVs
                EV_HP = 0,
                EV_Attack = 0,
                EV_Defense = 0,
                EV_SpecialAttack = 0,
                EV_SpecialDefense = 0,
                EV_Speed = 0,
            };

            // Deep-copy current moves into slots
            if (state.Moves != null)
            {
                for (int i = 0; i < Math.Min(state.Moves.Count, 4); i++)
                    playerPokemon.Moves[i] = ((MoveState)state.Moves[i]).Clone();
            }

            return playerPokemon;
        }
        // ─── Battle State ─────────────────────────────────────────────────────────

        /// <summary>Stat stage modifiers (-6 to +6) for Attack, Defense, SpAtk, SpDef, Speed, Accuracy, Evasion</summary>
        public int[] StatStages { get; private set; } = new int[7]; // Atk, Def, SpA, SpD, Spe, Acc, Eva

        public List<object> VolatileStatuses { get; set; } = new();  // swap object for your volatile-status type

        public int LastDamageDealt { get; set; }
        public int LastDamageTaken { get; set; }
        public int turnsActive { get; set; }

        // ─── Identity Shortcut ────────────────────────────────────────────────────

        /// <summary>Pokédex ID — delegates to the underlying state.</summary>
        public int PokedexId => PokemonState.PokedexId;

        // ─── Battle-State Reset Methods ───────────────────────────────────────────

        /// <summary>
        /// Resets all stat stages to 0 (used on switch-out or after battle).
        /// </summary>
        public void ResetStatStages()
        {
            for (int i = 0; i < StatStages.Length; i++)
                StatStages[i] = 0;
        }

        /// <summary>
        /// Clears persistent status condition.
        /// </summary>
        public void ClearStatus() => PersistentStatus = StatusCondition.None;
    }
}