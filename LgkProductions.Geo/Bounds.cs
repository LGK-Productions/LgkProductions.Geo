using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace LgkProductions.Geo;

/// <summary>
/// A class for saving generic value bounds. This ensures that Bounds.Min is smaller than Bounds.Max
/// </summary>
/// <typeparam name="T">The type of value to store</typeparam>
[Serializable]
public readonly record struct Bounds<T> where T : struct, INumber<T>
{
    private const string CastPattern = @"\s*\(\s*(\S+)\s*,\s*(\S+)\s*\)\s*";
    
    public T Min { get; init; }
    public T Max { get; init; }

    [JsonIgnore] public T Size => Max - Min;

    public Bounds(T Min, T Max)
    {
        if (Min > Max)
            (Min, Max) = (Max, Min);
        this.Min = Min;
        this.Max = Max;
    }
    
    /// <summary>
    /// Converts a string to a Bounds object, expecting the format (number, number)
    /// </summary>
    /// <param name="s">the input string</param>
    /// <returns>A Bounds object based on the string input</returns>
    public static Bounds<T> Parse(string s)
    {
        var match = Regex.Match(s, CastPattern);
        if (!match.Success) throw new FormatException();
        return new Bounds<T>(T.Parse(match.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture), T.Parse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture));
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