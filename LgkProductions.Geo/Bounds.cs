using System.Numerics;

namespace LgkProductions.Geo;

/// <summary>
/// A class for saving generic value bounds. This ensures that Bounds.Min is smaller than Bounds.Max
/// </summary>
/// <typeparam name="T">The type of value to store</typeparam>
public readonly record struct Bounds<T> where T : struct, INumber<T>
{
    public T Min { get; }
    public T Max { get; }

    public T Size => Max - Min;

    public Bounds(T min, T max)
    {
        if (min > max)
            (min, max) = (max, min);
        Min = min;
        Max = max;
    }

    public bool Contains(T value)
        => Min <= value && value <= Max;

    public bool Contains(Bounds<T> other)
        => Min <= other.Min && other.Max <= Max;

    public bool Overlaps(Bounds<T> other)
        => other.Min < Max && Min < other.Max;

    public T Clamp(T value)
    {
        if (value < Min)
            return Min;
        if (value > Max)
            return Max;
        return value;
    }
}