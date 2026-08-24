namespace Delta.Engine.Benchmarks;

internal sealed class BenchmarkRandom
{
    private uint _state;

    public BenchmarkRandom(int seed)
    {
        _state = unchecked((uint)seed);
        if (_state == 0)
        {
            _state = 1;
        }
    }

    public int Next(int minimum, int maximum)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(minimum, maximum);
        var range = (uint)(maximum - minimum);
        return minimum + (int)(NextUInt() % range);
    }

    public float NextSingle()
        => (NextUInt() >> 8) * (1.0f / 16_777_216.0f);

    private uint NextUInt()
    {
        _state = unchecked((_state * 1_664_525u) + 1_013_904_223u);
        return _state;
    }
}
