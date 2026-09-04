namespace OpenMoba.Sim;

/// <summary>
/// Internal PCG32 (XSH-RR) with 64-bit state and 32-bit output.
/// Matches the minimal C reference at https://www.pcg-random.org/using-pcg-c-basic.html
/// </summary>
internal struct Pcg32
{
    /// <summary>
    /// Fixed stream selector for Simulation Foundation.
    /// Chosen to match the PCG basic demo (initseq = 54) so Seed = 42 pins the public reference vector.
    /// </summary>
    public const ulong FoundationStream = 54;

    private ulong _state;
    private ulong _inc;

    public static Pcg32 Create(ulong seed, ulong stream = FoundationStream)
    {
        var rng = new Pcg32
        {
            _state = 0,
            _inc = (stream << 1) | 1,
        };
        rng.NextUInt32();
        rng._state += seed;
        rng.NextUInt32();
        return rng;
    }

    public uint NextUInt32()
    {
        var oldState = _state;
        _state = unchecked(oldState * 6364136223846793005UL + _inc);
        var xorshifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
        var rot = (int)(oldState >> 59);
        return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
    }
}
