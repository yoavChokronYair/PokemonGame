using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data.MapData;
using System;

namespace PokemonGameModel.Model.Helper
{
    internal class RandomPokemonIDHelper
    {
        public uint PID { get; }
        public ushort TrainerID { get; }
        public ushort SecretID { get; }

        public RandomPokemonIDHelper(uint pid, ushort tid, ushort sid)
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
        public static int GenerateIV()
        {
            return RandomHelper.Next(0, 32); // IV 0–31
        }

        public static int[] GenerateAllIVs()
        {
            return new int[]
            {
                GenerateIV(), // HP
                GenerateIV(), // Attack
                GenerateIV(), // Defense
                GenerateIV(), // Sp. Atk
                GenerateIV(), // Sp. Def
                GenerateIV()  // Speed
            };
        }

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
        public static string GenerateGender(double femaleRatio)
        {
            if (femaleRatio < 0) return "Genderless";
            return RandomHelper.NextBool(femaleRatio) ? "Female" : "Male";
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
        public int GetAbilityNumber()
        {
            // Ability 1 if PID is even, Ability 2 if PID is odd
            return (PID & 1) == 0 ? 1 : 2;
        }

        // ----------------------------
        // Full Pokémon Identity Generator
        // ----------------------------
        public static RandomPokemonIDHelper GenerateRandomPokemonIdentity()
        {
            ushort tid = GenerateRandomTID();
            ushort sid = GenerateRandomSID();
            uint pid = GeneratePID();
            return new RandomPokemonIDHelper(pid, tid, sid);
        }
        //encounters
        public Encounter? GetRandomEncounter(string routeName, string environment, List<Encounter> Encounters)
        {
            if (Encounters == null || Encounters.Count == 0)
                return null;

            // Filter by environment if necessary
            var filtered = Encounters
                .Where(e => e.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (filtered.Count == 0)
                return null;

            // Weighted random selection
            double totalRarity = filtered.Sum(e => e.Rarity);
            if (totalRarity <= 0)
                return null;

            double roll = RandomHelper.NextDouble() * totalRarity;
            double cumulative = 0.0;

            foreach (var spawn in filtered)
            {
                cumulative += spawn.Rarity;
                if (roll <= cumulative)
                    return spawn;
            }

            // fallback
            return filtered.Last();
        }

    }
}
