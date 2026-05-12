using PokemonGame.Core.Config;
using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Enums;
using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
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
                CurrentHP = maxHp,

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
    public static class PokemonConversionService
    {
        // ══════════════════════════════════════════════════════════════════════
        // PlayerTeam  →  PokemonTeam   (entering battle)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Builds a battle-ready PokemonTeam from the player's active team.
        /// The PokemonState objects are the same references, so post-battle
        /// sync only needs to copy the fields that battle can mutate.
        /// </summary>
        public static PokemonTeam ToBattleTeam(PlayerTeamDomain playerTeam)
        {
            var states = playerTeam.ActiveMembers
                .Select(p => p.PokemonState)
                .ToList();

            return PokemonTeam.Create(states);
        }


        // ══════════════════════════════════════════════════════════════════════
        // PokemonTeam  →  PlayerTeam   (post-battle sync)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// After a battle, writes all mutable battle fields from each
        /// PokemonState back into the matching PlayerPokemon.
        /// Matches by PokemonState reference identity (same object), so order
        /// in the two collections does not need to match.
        /// </summary>
        public static void SyncAfterBattle(
            PlayerTeamDomain playerTeam,
            PokemonTeam battleTeam,
            BattleReward reward)
        {
            // Build a quick lookup: PokemonState → PlayerPokemon
            var lookup = playerTeam.ActiveMembers
                .ToDictionary(p => p.PokemonState);

            foreach (var state in battleTeam.Members)
            {
                if (!lookup.TryGetValue(state, out var playerPokemon))
                    continue; // safety — should never happen

                // ── HP & Status (always mutated in battle) ────────────────────
                playerPokemon.CurrentHP = state.CurrentHP;
                playerPokemon.PersistentStatus = state.Status;

                // ── Move PP (consumed during battle) ──────────────────────────
                for (int i = 0; i < playerPokemon.Moves.Length; i++)
                {
                    if (playerPokemon.Moves[i] != null && i < state.Moves.Count)
                        playerPokemon.Moves[i]!.PP = ((MoveState)state.Moves[i]).PP;
                }
            }

            // ── Experience & EV gains ─────────────────────────────────────────
            foreach (var gain in reward.ExpGains)
            {
                if (!lookup.TryGetValue(gain.Target, out var playerPokemon))
                    continue;

                playerPokemon.Experience += gain.Amount;

                // Level-up loop
                while (playerPokemon.Experience >= playerPokemon.ExperienceToNextLevel
                       && playerPokemon.PokemonState.Level < 100)
                {
                    playerPokemon.Experience -= playerPokemon.ExperienceToNextLevel;
                    playerPokemon.PokemonState.Level++;
                }
            }

            foreach (var gain in reward.EvGains)
            {
                if (!lookup.TryGetValue(gain.Target, out var playerPokemon))
                    continue;

                ApplyEV(playerPokemon, gain.Stat, gain.Amount);
            }

            // ── Friendship (post-battle tick) ─────────────────────────────────
            foreach (var playerPokemon in playerTeam.ActiveMembers)
                playerPokemon.Friendship = MathHelper.Clamp(playerPokemon.Friendship + reward.FriendshipTick, 0, 255);
        }


        // ══════════════════════════════════════════════════════════════════════
        // WildPokemonDomain  →  PlayerPokemon   (after a successful catch)
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Creates a PlayerPokemon from a wild Pokémon that was just caught.
        /// Copies IVs/EVs from PokemonState into the flat PlayerPokemon fields
        /// so they can be edited independently (rare candies, vitamins, etc.).
        /// </summary>
        public static PokemonPlayerDomain FromWildCatch(
            WildPokemonDomain wild,
            string caughtOnRoute,
            PokeBallType ballUsed)
        {
            var state = wild.pokemonState;

            var playerPokemon = new PokemonPlayerDomain(wild, ObtainMethodType.Caught, caughtOnRoute)
            {
                CaughtWithBall = ballUsed,
                Friendship = wild.BaseFriendshipYield,

                // ── IVs (from state array if present) ─────────────────────────
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

            // Copy current moves into MoveSlots
            for (int i = 0; i < Math.Min(state.Moves.Count, 4); i++)
            {
                playerPokemon.Moves[i] = ((MoveState)state.Moves[i]).Clone();
            }

            return playerPokemon;
        }


        // ══════════════════════════════════════════════════════════════════════
        // Helpers
        // ══════════════════════════════════════════════════════════════════════

        private static void ApplyEV(PokemonPlayerDomain pokemon, Stat stat, int amount)
        {
            // Hard cap: no single stat above 252, total across all stats 510
            if (pokemon.TotalEVs >= 510) return;

            int headroom = Math.Min(510 - pokemon.TotalEVs, amount);

            switch (stat)
            {
                case Stat.HP: pokemon.EV_HP = Math.Min(252, pokemon.EV_HP + headroom); break;
                case Stat.Attack: pokemon.EV_Attack = Math.Min(252, pokemon.EV_Attack + headroom); break;
                case Stat.Defense: pokemon.EV_Defense = Math.Min(252, pokemon.EV_Defense + headroom); break;
                case Stat.SpecialAttack: pokemon.EV_SpecialAttack = Math.Min(252, pokemon.EV_SpecialAttack + headroom); break;
                case Stat.SpecialDefense: pokemon.EV_SpecialDefense = Math.Min(252, pokemon.EV_SpecialDefense + headroom); break;
                case Stat.Speed: pokemon.EV_Speed = Math.Min(252, pokemon.EV_Speed + headroom); break;
            }
        }
    }


    // ══════════════════════════════════════════════════════════════════════════
    // Supporting reward DTOs  (pure data, no logic)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Everything the battle system awards at the end of a fight.</summary>
    public class BattleReward
    {
        /// <summary>Exp points each participating Pokémon earns.</summary>
        public List<ExpGain> ExpGains { get; set; } = new();

        /// <summary>EV points each participating Pokémon earns.</summary>
        public List<EvGain> EvGains { get; set; } = new();

        /// <summary>Flat friendship added to every team member after the battle.</summary>
        public int FriendshipTick { get; set; } = 0;

        /// <summary>Money awarded to the player.</summary>
        public int MoneyAwarded { get; set; } = 0;
    }

    public class ExpGain
    {
        /// <summary>The PokemonState that participated (matched by reference in sync).</summary>
        public PokemonState Target { get; set; } = null!;
        public int Amount { get; set; }
    }

    public class EvGain
    {
        public PokemonState Target { get; set; } = null!;
        public Stat Stat { get; set; }
        public int Amount { get; set; }
    }
}

