using PokemonGameModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static uint GeneratePID()
        {
            return (uint)(RandomHelper.Next(int.MinValue, int.MaxValue));
        }

        // Gender Determination (example: 50% male/female)
        public bool IsMaleByFemalePercent(double femalePercent)
        {
            if (femalePercent < 0 || femalePercent > 100)
                throw new ArgumentOutOfRangeException(nameof(femalePercent), "Percentage must be 0-100.");

            int randomValue = RandomHelper.Next(0, 100); // 0 to 99

            bool isFemale = randomValue < femalePercent;
            return !isFemale; // return true if male
        }
        public static ushort GenerateRandomSID()
        {
            return (ushort)RandomHelper.Next(0, 65536); // Generate a random Secret ID (0-65535)
        }
        // Ability Determination (odd/even PID)
        public int GetAbilityNumber()
        {
            return (PID & 1) == 0 ? 1 : 2;
        }

        // Nature Determination (PID mod 25)
        public NatureType GetNature()
        {
            Array values = Enum.GetValues(typeof(NatureType));
            return (NatureType)values.GetValue(PID % 25);
        }

        // Shininess Calculation
        public bool IsShiny()
        {
            ushort tid = TrainerID;
            ushort sid = SecretID;
            ushort pidHigh = (ushort)(PID >> 16);
            ushort pidLow = (ushort)(PID & 0xFFFF);

            ushort shinyValue = (ushort)((tid ^ sid ^ pidHigh ^ pidLow));
            return shinyValue < 8;
        }
    }
}
