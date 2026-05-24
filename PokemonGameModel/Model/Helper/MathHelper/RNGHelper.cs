// Design: Static factory + Instance value object.
// Layer: Model/Helper/MathHelper — game-specific RNG (PID, IVs, nature, gender, shininess).
// Uses RandomHelper for all random number generation — no inline new Random() here.
// Instance holds PID/TID/SID for shiny and gender checks.
using PokemonGame.Enums;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Item;
using PokemonGame.Model.Domain.Map;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Model.Battle;

namespace PokemonGame.Core.Model.Helper.MathHelper
{
    //class helper for RNG game specific calculations
    internal class RNGHelper
    {
        public uint PID { get; }
        public ushort TrainerID { get; }
        public ushort SecretID { get; }

        public RNGHelper(uint pid, ushort tid, ushort sid)
        {
            PID = pid;
            TrainerID = tid;
            SecretID = sid;
        }

        // ----------------------------
        // PID / IDs
        // ----------------------------
        public static uint GeneratePID()
        {
            return (uint)RandomHelper.Next(int.MinValue, int.MaxValue); // 32-bit PID
        }

        public static ushort GenerateRandomTID()
        {
            return (ushort)RandomHelper.Next(0, 65536); // Trainer ID (0–65535)
        }

        public static ushort GenerateRandomSID()
        {
            return (ushort)RandomHelper.Next(0, 65536); // Secret ID (0–65535)
        }

        // ----------------------------
        // IVs
        // ----------------------------
        public static int GenerateIV(int? baseIV = null)
        {
            // If base IV is defined (>= 0), use it directly
            if (baseIV.HasValue && baseIV.Value >= 0)
            {
                return baseIV.Value;
            }

            // Otherwise, random IV between 0 and 31
            return RandomHelper.Next(0, 32);
        }

        /// <summary>
        /// Generates IVs for all 6 stats. If the Pokémon species has base IVs defined, they’re respected.
        /// </summary>
        //public static StatValues GenerateAllIVs(PokemonData? pokemon = null)
        //{
        //    var baseIVs = pokemon?.IVs;

        //    return new StatValues
        //    {
        //        HP = GenerateIV(baseIVs?.HP),
        //        Attack = GenerateIV(baseIVs?.Attack),
        //        Defense = GenerateIV(baseIVs?.Defense),
        //        SpecialAttack = GenerateIV(baseIVs?.SpecialAttack),
        //        SpecialDefense = GenerateIV(baseIVs?.SpecialDefense),
        //        Speed = GenerateIV(baseIVs?.Speed)
        //    };
        //}


        // ----------------------------
        // Nature
        // ----------------------------
        public static NatureType GenerateNature()
        {
            Array values = Enum.GetValues(typeof(NatureType));
            return (NatureType)values.GetValue(RandomHelper.Next(0, values.Length));
        }




        // ----------------------------
        // Shininess
        // ----------------------------
        public bool IsShiny()
        {
            int shinyValue = TrainerID ^ SecretID ^ ((int)PID & 0xFFFF) ^ ((int)PID >> 16);
            return shinyValue < 8; // 1/8192 chance
        }

        // ----------------------------
        // Ability Determination
        // ----------------------------
        //public int GetAbilityNumber(PokemonData pokemon)
        //{
        //    // 0–15 range: if result == 0, Hidden Ability (1/16 chance)
        //    int hiddenRoll = RandomHelper.Next(0,16);

        //    if (hiddenRoll == 0 && pokemon.Abilitys.Count > 2)
        //        return 3; // Hidden Ability

        //    // Otherwise pick based on PID parity
        //    return (PID & 1) == 0 ? 1 : 2;
        //}
        // ----------------------------
        // Gender check
        // ----------------------------
        public Gender IsFemale(double femaleRatio)
        {
            // Handle Genderless cases (usually represented by -1.0)
            if (femaleRatio < 0)
            {
                return Gender.Genderless;
            }

            // Convert the 0.0-1.0 ratio to a 0-255 threshold
            // Example: 0.125 (12.5% female) becomes 32
            int genderThreshold = (int)(femaleRatio * 256);

            // Get the lowest 8 bits of the Personal ID (PID)
            int pidLowByte = (int)(PID & 0xFF);

            // If the PID byte is lower than the threshold, it's female
            return (pidLowByte < genderThreshold) ? Gender.Female : Gender.Male;
        }
        // ----------------------------
        // Nature based on PID
        // ----------------------------
        public NatureType GetNature()
        {
            int natureIndex = (int)(PID % 25);
            return (NatureType)natureIndex;
        }

        // ----------------------------
        // Full Pokémon Identity Generator
        // ----------------------------
        public static RNGHelper GenerateRandomPokemonIdentity(int trainerID)
        {
            ushort tid = (ushort)trainerID;
            ushort sid = GenerateRandomSID();
            uint pid = GeneratePID();
            return new RNGHelper(pid, tid, sid);
        }

        public static double GetCritModifier(
            BattleLogger logger,
            int pokemonCritStage = 0,
            int moveCritStage = 0)
        {
            int totalCritStage = PokemonGame.Model.Helper.MathHelper.Clamp(
                pokemonCritStage + moveCritStage,
                0,
                4);

            int critChanceDenominator = totalCritStage switch
            {
                0 => 24, // normal crit rate
                1 => 8,
                2 => 2,
                3 => 1,
                4 => 1,
                _ => 24
            };

            bool isCrit = RandomHelper.Next(0, critChanceDenominator) == 0;

            if (isCrit)
            {
                logger.Log("A critical hit!");
                return 2.0;
            }

            return 1.0;
        }
        public static bool TryWildEncounter(int encounterRate)
        {
            return RandomHelper.Next(1, 101) <= encounterRate;
        }
        public static EncounterDomain? PickWildEncounter(IEnumerable<EncounterDomain> entries)
        {
            var list = entries as IList<EncounterDomain> ?? entries.ToList();

            if (list.Count == 0) return null;

            int totalWeight = list.Sum(e => e.Rate);
            int roll = RandomHelper.Next(1, totalWeight + 1);

            int cumulative = 0;
            foreach (var entry in list)
            {
                cumulative += entry.Rate;
                if (roll <= cumulative)
                    return entry;
            }

            return list[0]; // fallback — should never reach here
        }
        public static bool CanEscapeWildEncounter(PokemonState pokemon, PokemonState wildPokemon, int attempt)
        {
            int escapeChance = ((pokemon.BaseSpeed * 128) / wildPokemon.BaseSpeed) + 30 * attempt;
            return RandomHelper.Next(0, 256) < escapeChance;
        }
        public class CatchResult
        {
            public bool Caught { get; set; }
            public int ShakeCount { get; set; }
            public double A { get; set; }
            public double B { get; set; }
            public double BallMultiplier { get; set; }
            public double StatusModifier { get; set; }
        }

        public static CatchResult RollCatch(
            WildPokemonDomain wildPokemon,
            PokeballState ball,
            BattleState battle)
        {
            if (wildPokemon == null)
                throw new ArgumentNullException(nameof(wildPokemon));

            if (ball == null)
                throw new ArgumentNullException(nameof(ball));

            var pokemon = wildPokemon.pokemonState;

            int catchRate = wildPokemon.CatchRate;
            int maxHp = pokemon.MaxHP;
            int currentHp = pokemon.CurrentHP;

            if (catchRate <= 0 || maxHp <= 0)
            {
                return new CatchResult
                {
                    Caught = false,
                    ShakeCount = 0,
                    A = 0,
                    B = 0,
                    BallMultiplier = 0,
                    StatusModifier = 1.0
                };
            }

            currentHp = PokemonGame.Model.Helper.MathHelper.Clamp(
                currentHp,
                1,
                maxHp);

            double ballMultiplier = ball.GetEffectiveMultiplier(battle);

            double statusModifier = pokemon.Status switch
            {
                StatusCondition.Sleep or StatusCondition.Freeze => 2.0,

                StatusCondition.Paralysis
                    or StatusCondition.Burn
                    or StatusCondition.Poison
                    or StatusCondition.Toxic => 1.5,

                _ => 1.0
            };

            double a =
                ((3.0 * maxHp - 2.0 * currentHp)
                 * catchRate
                 * ballMultiplier
                 * statusModifier)
                / (3.0 * maxHp);

            // BUG-103 fix:
            // Do NOT clamp minimum to 1.
            // If a <= 0, catch is impossible.
            if (a <= 0)
            {
                return new CatchResult
                {
                    Caught = false,
                    ShakeCount = 0,
                    A = a,
                    B = 0,
                    BallMultiplier = ballMultiplier,
                    StatusModifier = statusModifier
                };
            }

            // Gen III/IV rule:
            // a >= 255 means guaranteed catch.
            if (a >= 255)
            {
                return new CatchResult
                {
                    Caught = true,
                    ShakeCount = 4,
                    A = a,
                    B = 65535,
                    BallMultiplier = ballMultiplier,
                    StatusModifier = statusModifier
                };
            }

            double b = 65536.0 / Math.Pow(255.0 / a, 0.1875);

            int shakes = 0;

            for (int i = 0; i < 4; i++)
            {
                if (RandomHelper.Next(0, 65536) >= (int)b)
                {
                    return new CatchResult
                    {
                        Caught = false,
                        ShakeCount = shakes,
                        A = a,
                        B = b,
                        BallMultiplier = ballMultiplier,
                        StatusModifier = statusModifier
                    };
                }

                shakes++;
            }

            return new CatchResult
            {
                Caught = true,
                ShakeCount = 4,
                A = a,
                B = b,
                BallMultiplier = ballMultiplier,
                StatusModifier = statusModifier
            };
        }
    }
}
