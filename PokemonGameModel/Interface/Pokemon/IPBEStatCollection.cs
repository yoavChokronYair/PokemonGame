// Layer: Interface — stat collection contracts for PBE (Pokemon Battle Engine).
// IPBEReadOnlyStatCollection: read-only view (used for IVs).
// IPBEStatCollection: read/write view (used for EVs).

namespace PokemonGame.Interface.Pokemon
{
    public interface IPBEReadOnlyStatCollection
    {
        byte HP { get; }
        byte Attack { get; }
        byte Defense { get; }
        byte SpAttack { get; }
        byte SpDefense { get; }
        byte Speed { get; }
    }
    public interface IPBEStatCollection : IPBEReadOnlyStatCollection
    {
        new byte HP { get; set; }
        new byte Attack { get; set; }
        new byte Defense { get; set; }
        new byte SpAttack { get; set; }
        new byte SpDefense { get; set; }
        new byte Speed { get; set; }
    }
}
