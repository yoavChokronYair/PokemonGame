using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Model.Battle;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Translators;

namespace PokemonGame.Tests
{
    // ── Fake Move Service ─────────────────────────────────────────────────────
    // Returns real-mechanics trees for Hyper Beam, Tackle, and Thunderbolt.
    // No DB, no SQLite.
    internal class FakeBattleMoveService : IMoveService
    {
        public MoveTree? GetMove(string name) => name switch
        {
            "HyperBeam" => BuildHyperBeam(),
            "Tackle" => BuildTackle(),
            "Thunderbolt" => BuildThunderbolt(),
            _ => null,
        };

        // ── Hyper Beam ────────────────────────────────────────────────────────
        // 150-power Normal Special, always hits, then a Cascade recharge turn.
        private static MoveTree BuildHyperBeam()
        {
            var move = new MoveData
            {
                Id = 20,
                Name = "HyperBeam",
                Element = "Normal",
                Category = "Special",
                Target = "Opponent",
                PP = 5,
                Priority = 0,
                CritStage = 0,
                Description = "The target is attacked with a powerful beam. The user must rest on the next turn.",
            };

            // Step 1 — deal 150-power formula damage
            var damageEffect = new MoveEffect
            {
                Id = 201,
                Type = "FormulaDamage",
                Target = "Defender",
                Number = new MoveNumber { Id = 21, Type = "Exactly", ExactValue = 150.0 },
            };

            // Step 2 — recharge: NoEffect (recharge mechanic lives in Charge attempt type;
            //          here we model it as a Cascade: hit attempt then a recharge attempt).
            var rechargeEffect = new MoveEffect
            {
                Id = 202,
                Type = "NoEffect",
            };

            // Hit attempt — accuracy 1.0, on-hit = FormulaDamage(150)
            var hitAttempt = new MoveAttempt
            {
                Id = 21,
                Type = "Attempt",
                AccuracyValue = 1.0,
                OnHit = damageEffect,
            };

            // Recharge attempt — wrapped in a Charge attempt so the release is the real hit
            // (Cascade: [hitAttempt] represents turn 1; recharge = Charge with NoEffect charge)
            // We use a Charge attempt: charge=NoEffect, release=hitAttempt
            var chargeAttempt = new MoveAttempt
            {
                Id = 22,
                Type = "Charge",
                ChargeEffect = rechargeEffect,
                ReleaseAttempt = hitAttempt,
            };

            return new MoveTree
            {
                Move = move,
                Priority = 0,
                CritStage = 0,
                Description = move.Description,
                Attempts = new List<MoveAttempt> { chargeAttempt },
            };
        }

        // ── Tackle ────────────────────────────────────────────────────────────
        // 40-power Normal Physical, accuracy 1.0, no side-effect.
        private static MoveTree BuildTackle()
        {
            var move = new MoveData
            {
                Id = 30,
                Name = "Tackle",
                Element = "Normal",
                Category = "Physical",
                Target = "Opponent",
                PP = 35,
                Priority = 0,
                CritStage = 0,
                Description = "A physical attack in which the user charges and slams into the target.",
            };

            var damageEffect = new MoveEffect
            {
                Id = 301,
                Type = "FormulaDamage",
                Target = "Defender",
                Number = new MoveNumber { Id = 31, Type = "Exactly", ExactValue = 40.0 },
            };

            var attempt = new MoveAttempt
            {
                Id = 31,
                Type = "Attempt",
                AccuracyValue = 1.0,
                OnHit = damageEffect,
            };

            return new MoveTree
            {
                Move = move,
                Priority = 0,
                CritStage = 0,
                Description = move.Description,
                Attempts = new List<MoveAttempt> { attempt },
            };
        }

        // ── Thunderbolt ───────────────────────────────────────────────────────
        // 90-power Electric Special, accuracy 1.0, 10% chance to paralyze.
        private static MoveTree BuildThunderbolt()
        {
            var move = new MoveData
            {
                Id = 40,
                Name = "Thunderbolt",
                Element = "Electric",
                Category = "Special",
                Target = "Opponent",
                PP = 15,
                Priority = 0,
                CritStage = 0,
                Description = "A strong electric blast crashes down on the target. It may also leave the target with paralysis.",
            };

            var paralyzeEffect = new MoveEffect
            {
                Id = 403,
                Type = "Paralyze",
                Target = "Defender",
            };

            var chanceEffect = new MoveEffect
            {
                Id = 402,
                Type = "Chance",
                ChanceProbability = 0.10,
                ChanceChild = paralyzeEffect,
            };

            var damageEffect = new MoveEffect
            {
                Id = 401,
                Type = "FormulaDamage",
                Target = "Defender",
                Number = new MoveNumber { Id = 41, Type = "Exactly", ExactValue = 90.0 },
            };

            var sequenceEffect = new MoveEffect
            {
                Id = 400,
                Type = "Sequence",
                SequenceSteps = new List<MoveEffect> { damageEffect, chanceEffect },
            };

            var attempt = new MoveAttempt
            {
                Id = 41,
                Type = "Attempt",
                AccuracyValue = 1.0,
                OnHit = sequenceEffect,
            };

            return new MoveTree
            {
                Move = move,
                Priority = 0,
                CritStage = 0,
                Description = move.Description,
                Attempts = new List<MoveAttempt> { attempt },
            };
        }
    }
    // ── Test factory helpers ──────────────────────────────────────────────────
    internal static class BattleTestFactory
    {
        private static readonly IReadOnlyList<string> PlayerMoves =
            new[] { "HyperBeam", "Thunderbolt" };

        private static readonly IReadOnlyList<string> EnemyMoves =
            new[] { "Tackle", "Thunderbolt" };

        public static MoveTranslator MoveTranslator() =>
            new MoveTranslator(new FakeBattleMoveService());
        public static AbilityTranslator AbilityTranslator() =>
            new AbilityTranslator(); // No abilities in this test, so no fake service needed.
        public static ItemTranslator itemTranslator() => new ItemTranslator();

        public static TeamTranslator TeamTranslator() =>
            new TeamTranslator(new FakePokemonService(PlayerMoves, EnemyMoves),
                               MoveTranslator(), AbilityTranslator(), itemTranslator());

        public static PokemonTeam PlayerTeam() =>
            TeamTranslator().LoadTeam(battlePlayerId: 1);

        public static PokemonTeam EnemyTeam() =>
            TeamTranslator().LoadTeam(battlePlayerId: 2);

       
    }
}