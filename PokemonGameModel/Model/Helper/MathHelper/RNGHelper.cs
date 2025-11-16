using PokemonGame.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Services.Data.Pokemon;
using PokemonGame.Services.Enums.PokemonEnum;
using System;

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
                return baseIV.Value;

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
        // Gender
        // ----------------------------
        /// <summary>
        /// ratio = chance of female (0.0–1.0), -1 = genderless.
        /// </summary>
        /// 
        private static double GetFemaleRatio(GenderRatioType ratio)
        {
            return ratio switch
            {
                GenderRatioType.M7_F1 => 0.125, // 12.5%
                GenderRatioType.M3_F1 => 0.25,  // 25%
                GenderRatioType.M1_F1 => 0.5,   // 50%
                GenderRatioType.M1_F3 => 0.75,  // 75%
                GenderRatioType.M0_F1 => 1.0,   // 100% female
                GenderRatioType.M1_F0 => 0.0,   // 0% female
                GenderRatioType.M0_F0 => -1.0,  // genderless
                _ => -1.0
            };
        }
        public static GenderType GenerateGender(GenderRatioType ratio)
        {
            double femaleRatio = GetFemaleRatio(ratio);

            if (femaleRatio < 0)
                return GenderType.Genderless;

            return RandomHelper.NextBool(femaleRatio) ? GenderType.Female : GenderType.Male;
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
        public bool IsFemale(double femaleRatio)
        {
            // Genderless
            if (femaleRatio < 0) return false;

            // PID determines gender
            int genderThreshold = (int)(femaleRatio * 256);
            int pidLowByte = (int)(PID & 0xFF); // lowest 8 bits of PID

            return pidLowByte < genderThreshold;
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
        public static RNGHelper GenerateRandomPokemonIdentity()
        {
            ushort tid = GenerateRandomTID();
            ushort sid = GenerateRandomSID();
            uint pid = GeneratePID();
            return new RNGHelper(pid, tid, sid);
        }
      

    }
}
