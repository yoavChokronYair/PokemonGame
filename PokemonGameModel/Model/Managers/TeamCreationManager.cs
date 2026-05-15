using PokemonGame.Core.Config;
using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Enums;
using PokemonGame.Model.Config;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Player;
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

        // ── Base Stats ───────────────────────────────────────────────
        public int BaseHp { get; set; }
        public int BaseAtk { get; set; }
        public int BaseDef { get; set; }
        public int BaseSpAtk { get; set; }
        public int BaseSpDef { get; set; }
        public int BaseSpeed { get; set; }

        // ── IVs ──────────────────────────────────────────────────────
        public int IvHp { get; set; }
        public int IvAtk { get; set; }
        public int IvDef { get; set; }
        public int IvSpAtk { get; set; }
        public int IvSpDef { get; set; }
        public int IvSpeed { get; set; }

        // ── EVs ──────────────────────────────────────────────────────
        public int EvHp { get; set; }
        public int EvAtk { get; set; }
        public int EvDef { get; set; }
        public int EvSpAtk { get; set; }
        public int EvSpDef { get; set; }
        public int EvSpeed { get; set; }

        public List<IMove> Moves { get; set; } = new();

        public IAbility Ability { get; set; } = null!;
        public IHeldItem? HeldItem { get; set; }

        public GrowthRateType GrowthRate { get; set; } =
            GrowthRateType.MediumFast;
    }

    public class TeamCreationManager
    {
        public PokemonTeam BuildTeam(
            IReadOnlyList<PokemonCreationData> roster)
        {
            if (roster == null || roster.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot build a team from an empty roster.");
            }

            if (roster.Count > PokemonConstants.PartyCapacity)
            {
                throw new InvalidOperationException(
                    $"Expected maximum {PokemonConstants.PartyCapacity} members, found {roster.Count}.");
            }

            var states = roster
                .Select(BuildPokemon)
                .ToList();

            return PokemonTeam.Create(states);
        }

        public PokemonState BuildPokemon(PokemonCreationData data)
        {
            NatureType nature =
                (NatureType)Enum.Parse(
                    typeof(NatureType),
                    data.Nature,
                    true);

            var natureModifiers =
                NatureConstants.GetNatureModifiers(nature);

            int maxHp =
                PokemonStatCalculatorHelper.CalculateHP(
                    data.BaseHp,
                    data.IvHp,
                    data.EvHp,
                    data.Level);

            return new PokemonState
            {
                Name = data.Name,

                PokedexId = data.PokedexId,

                PrimaryType =
                    (PokemonType)Enum.Parse(
                        typeof(PokemonType),
                        data.Type1,
                        true),

                SecondaryType =
                    !string.IsNullOrWhiteSpace(data.Type2)
                        ? (PokemonType?)Enum.Parse(
                            typeof(PokemonType),
                            data.Type2,
                            true)
                        : null,

                Level = data.Level,

                Nature = nature,

                Base = new BaseStats(
                    data.BaseHp,
                    data.BaseAtk,
                    data.BaseDef,
                    data.BaseSpAtk,
                    data.BaseSpDef,
                    data.BaseSpeed),

                MaxHP = maxHp,
                CurrentHP = maxHp,

                BaseAttack =
                    PokemonStatCalculatorHelper.CalculateStat(
                        data.BaseAtk,
                        data.IvAtk,
                        data.EvAtk,
                        data.Level,
                        natureModifiers.atk),

                BaseDefense =
                    PokemonStatCalculatorHelper.CalculateStat(
                        data.BaseDef,
                        data.IvDef,
                        data.EvDef,
                        data.Level,
                        natureModifiers.def),

                BaseSpecialAttack =
                    PokemonStatCalculatorHelper.CalculateStat(
                        data.BaseSpAtk,
                        data.IvSpAtk,
                        data.EvSpAtk,
                        data.Level,
                        natureModifiers.spAtk),

                BaseSpecialDefense =
                    PokemonStatCalculatorHelper.CalculateStat(
                        data.BaseSpDef,
                        data.IvSpDef,
                        data.EvSpDef,
                        data.Level,
                        natureModifiers.spDef),

                BaseSpeed =
                    PokemonStatCalculatorHelper.CalculateStat(
                        data.BaseSpeed,
                        data.IvSpeed,
                        data.EvSpeed,
                        data.Level,
                        natureModifiers.speed),

                IVs = new[]
                {
            data.IvHp,
            data.IvAtk,
            data.IvDef,
            data.IvSpAtk,
            data.IvSpDef,
            data.IvSpeed
        },

                EVs = new[]
                {
            data.EvHp,
            data.EvAtk,
            data.EvDef,
            data.EvSpAtk,
            data.EvSpDef,
            data.EvSpeed
        },

                Moves = data.Moves,

                Ability = data.Ability,

                HeldItem = data.HeldItem
            };
        }
    }

    public static class PokemonConversionService
    {
        // ══════════════════════════════════════════════════════════════
        // PlayerTeam → BattleTeam
        // ══════════════════════════════════════════════════════════════

        public static PokemonTeam ToBattleTeam(
            PlayerTeamDomain playerTeam)
        {
            var states = playerTeam.ActiveMembers
                .Select(p => p.PokemonState)
                .ToList();

            return PokemonTeam.Create(states);
        }
        public static PokemonState ToBattleWild(
            WildPokemonDomain wild,
            PokemonCreationData baseData)
        {
            baseData.Level = wild.pokemonState.Level;

            baseData.PokedexId = wild.pokemonState.PokedexId;

            return new TeamCreationManager().BuildPokemon(baseData);
        }

        // ══════════════════════════════════════════════════════════════
        // Sync Battle Results
        // ══════════════════════════════════════════════════════════════

        public static void SyncAfterBattle(
            PlayerTeamDomain playerTeam,
            PokemonTeam battleTeam,
            BattleReward reward)
        {
            var lookup = playerTeam.ActiveMembers
                .ToDictionary(x => x.PokemonState);

            // ── Sync battle state ───────────────────────────────────
            foreach (var state in battleTeam.Members)
            {
                if (!lookup.TryGetValue(state, out var playerPokemon))
                    continue;

                playerPokemon.CurrentHP = state.CurrentHP;

                playerPokemon.PersistentStatus = state.Status;

                // Sync PP
                for (int i = 0; i < playerPokemon.Moves.Length; i++)
                {
                    if (playerPokemon.Moves[i] != null &&
                        i < state.Moves.Count)
                    {
                        playerPokemon.Moves[i]!.PP =
                            ((MoveState)state.Moves[i]).PP;
                    }
                }
            }

            // ── Experience ──────────────────────────────────────────
            foreach (var gain in reward.ExpGains)
            {
                if (!lookup.TryGetValue(gain.Target, out var playerPokemon))
                    continue;

                LevelUpResult result =
                    playerPokemon.GainExperience(gain.Amount);

                // Optional hook for UI/dialogue
                HandleLevelUpResult(playerPokemon, result);
            }

            // ── EV Gains ────────────────────────────────────────────
            foreach (var gain in reward.EvGains)
            {
                if (!lookup.TryGetValue(gain.Target, out var playerPokemon))
                    continue;

                ApplyEV(playerPokemon, gain.Stat, gain.Amount);
            }

            // ── Friendship ──────────────────────────────────────────
            foreach (var pokemon in playerTeam.ActiveMembers)
            {
                pokemon.Friendship =
                    MathHelper.Clamp(
                        pokemon.Friendship + reward.FriendshipTick,
                        0,
                        255);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // Wild Catch
        // ══════════════════════════════════════════════════════════════

        public static PokemonPlayerDomain FromWildCatch(
            WildPokemonDomain wild,
            string caughtOnRoute,
            PokeBallType ballUsed)
        {
            return PokemonPlayerDomain.FromWildCatch(
                wild,
                caughtOnRoute,
                ballUsed);
        }

        // ══════════════════════════════════════════════════════════════
        // EV Logic
        // ══════════════════════════════════════════════════════════════

        private static void ApplyEV(
            PokemonPlayerDomain pokemon,
            Stat stat,
            int amount)
        {
            switch (stat)
            {
                case Stat.Attack:
                    pokemon.EV_Attack += amount;
                    break;

                case Stat.Defense:
                    pokemon.EV_Defense += amount;
                    break;

                case Stat.SpecialAttack:
                    pokemon.EV_SpecialAttack += amount;
                    break;

                case Stat.SpecialDefense:
                    pokemon.EV_SpecialDefense += amount;
                    break;

                case Stat.Speed:
                    pokemon.EV_Speed += amount;
                    break;

                default:
                    pokemon.EV_HP += amount;
                    break;
            }

            pokemon.NormalizeEVs();
        }

        // ══════════════════════════════════════════════════════════════
        // UI/Event Hook
        // ══════════════════════════════════════════════════════════════

        private static void HandleLevelUpResult(
            PokemonPlayerDomain pokemon,
            LevelUpResult result)
        {
            // Example:
            //
            // foreach (var lvl in result.GainedLevels)
            // {
            //     Console.WriteLine($"{pokemon.PokemonState.Name} grew to Lv.{lvl}");
            // }
            //
            // foreach (var move in result.LearnedMoves)
            // {
            //     Console.WriteLine($"{pokemon.PokemonState.Name} learned {move.Move.Name}");
            // }
            //
            // if (result.Evolved)
            // {
            //     Console.WriteLine($"{pokemon.PokemonState.Name} is evolving!");
            // }
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // Battle Reward DTOs
    // ══════════════════════════════════════════════════════════════════

    public class BattleReward
    {
        public List<ExpGain> ExpGains { get; set; } = new();

        public List<EvGain> EvGains { get; set; } = new();

        public int FriendshipTick { get; set; }

        public int MoneyAwarded { get; set; }
    }

    public class ExpGain
    {
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