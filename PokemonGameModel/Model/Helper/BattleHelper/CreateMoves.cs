using PokemonGame.Interface;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    internal class CreateMoves
    {
        public static IMove Tackle() => new MoveDomain(
            name: "Tackle",
            element: PokemonType.Normal,
            category: MoveCategory.Physical,
            attempt: new Attempt(
                accuracy: new Probability(0.95),
                onHit: new FormulaDamage(new DefenderTarget(), new Exactly(40)),
                onMiss: null,
                after: null
            ),
            pp: 35
        );

        public static IMove BodySlam() => new MoveDomain(
            name: "Body Slam",
            element: PokemonType.Normal,
            category: MoveCategory.Physical,
            attempt: new Attempt(
                accuracy: new Probability(1.0),
                onHit: new Sequence(
                    new FormulaDamage(new DefenderTarget(), new Exactly(85)),
                    new Chance(0.30, new Paralyze(new DefenderTarget()))
                ),
                onMiss: null,
                after: null
            ),
            pp: 15
        );

        // Each kick has independent accuracy, and each hit's power increases by 10
        // Kick 1 = 10, Kick 2 = 20, Kick 3 = 30
        public static IMove TripleKick() => new MoveDomain(
            name: "Triple Kick",
            element: PokemonType.Fighting,
            category: MoveCategory.Physical,
            attempt: new Cascade(
                stopOnMiss: true,
                new Attempt(new Probability(0.90), new FormulaDamage(new DefenderTarget(), new Exactly(10)), null, null),
                new Attempt(new Probability(0.90), new FormulaDamage(new DefenderTarget(), new Exactly(20)), null, null),
                new Attempt(new Probability(0.90), new FormulaDamage(new DefenderTarget(), new Exactly(30)), null, null)
            ),
            pp: 10
        );

        // Crashes for half of user's max HP on miss
        public static IMove JumpKick() => new MoveDomain(
            name: "Jump Kick",
            element: PokemonType.Fighting,
            category: MoveCategory.Physical,
            attempt: new Attempt(
                accuracy: new Probability(0.95),
                onHit: new FormulaDamage(new DefenderTarget(), new Exactly(100)),
                onMiss: new CrashDamage(new AttackerTarget(), new Product(new MaxHP(new AttackerTarget()), new Exactly(0.5))),
                after: null
            ),
            pp: 10
        );

        public static IMove SolarBeam() => new MoveDomain(
            name: "Solar Beam",
            element: PokemonType.Grass,
            category: MoveCategory.Special,
            attempt: new Charge(
                chargeEffect: new NoEffect(),
                releaseAttempt: new Attempt(
                    accuracy: new Probability(1.0),
                    onHit: new FormulaDamage(new DefenderTarget(), new Exactly(120)),
                    onMiss: null,
                    after: null
                )
            ),
            pp: 10
        );

        public static IMove BulletSeed() => new MoveDomain(
            name: "Bullet Seed",
            element: PokemonType.Grass,
            category: MoveCategory.Physical,
            attempt: new Combo(
                accuracy: new Probability(1.0),
                hits: new Weighted(new List<(double, double)> { (2, 35), (3, 35), (4, 15), (5, 15) }),
                onEachHit: new FormulaDamage(new DefenderTarget(), new Exactly(25)),
                onEachMiss: null,
                after: null
            ),
            pp: 30
        );

        public static IMove Flamethrower() => new MoveDomain(
            name: "Flamethrower",
            element: PokemonType.Fire,
            category: MoveCategory.Special,
            attempt: new Attempt(
                accuracy: new Probability(1.0),
                onHit: new Sequence(
                    new FormulaDamage(new DefenderTarget(), new Exactly(90)),
                    new Chance(0.10, new Burn(new DefenderTarget()))
                ),
                onMiss: null,
                after: null
            ),
            pp: 15
        );

        public static IMove Thunderbolt() => new MoveDomain(
            name: "Thunderbolt",
            element: PokemonType.Electric,
            category: MoveCategory.Special,
            attempt: new Attempt(
                accuracy: new Probability(1.0),
                onHit: new Sequence(
                    new FormulaDamage(new DefenderTarget(), new Exactly(90)),
                    new Chance(0.10, new Paralyze(new DefenderTarget()))
                ),
                onMiss: null,
                after: null
            ),
            pp: 15
        );

        public static IMove Blizzard() => new MoveDomain(
            name: "Blizzard",
            element: PokemonType.Ice,
            category: MoveCategory.Special,
            attempt: new Attempt(
                accuracy: new Probability(0.70),
                onHit: new Sequence(
                    new FormulaDamage(new DefenderTarget(), new Exactly(110)),
                    new Chance(0.10, new Freeze(new DefenderTarget()))
                ),
                onMiss: null,
                after: null
            ),
            pp: 5
        );

        public static IMove DoubleEdge() => new MoveDomain(
            name: "Double-Edge",
            element: PokemonType.Normal,
            category: MoveCategory.Physical,
            attempt: new Attempt(
                accuracy: new Probability(1.0),
                onHit: new Sequence(
                    new FormulaDamage(new DefenderTarget(), new Exactly(120)),
                    new Recoil(new AttackerTarget(), new Quotient(new LastDamageDealt(new AttackerTarget()), new Exactly(3)))
                ),
                onMiss: null,
                after: null
            ),
            pp: 15
        );

        public static IMove SwordsDance() => new MoveDomain(
            name: "Swords Dance",
            element: PokemonType.Normal,
            category: MoveCategory.Status,
            attempt: new Attempt(
                accuracy: new Probability(1.0),
                onHit: new StatChange(new AttackerTarget(), Stat.Attack, +2),
                onMiss: null,
                after: null
            ),
            pp: 20
        );

        public static IMove Recover() => new MoveDomain(
            name: "Recover",
            element: PokemonType.Normal,
            category: MoveCategory.Status,
            attempt: new Attempt(
                accuracy: new Probability(1.0),
                onHit: new RestoreHP(
                    new AttackerTarget(),
                    new Quotient(new MaxHP(new AttackerTarget()), new Exactly(2))
                ),
                onMiss: null,
                after: null
            ),
            pp: 10
        );

    }
}