using System.Globalization;
using System.Text.RegularExpressions;

namespace LgkProductions.Geo;

/// <summary>
/// A data class storing the position of a point on the globe in latitude, longitude and altitude.
/// </summary>
public readonly record struct GlobePoint
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double Altitude { get; init; }

    static readonly Bounds<double> LatitudeBounds = new(-90.0, 90.0);
    static readonly Bounds<double> LongitudeBounds = new(-180.0, 180.0);
    
    private const string CastPattern = @"\s*\(\s*(\S+)\s*,\s*(\S+)\s*(,\s*(\S+)\s*)?\)\s*";

    /// <summary>
    /// The latitude as a DMS string
    /// </summary>
    public string DmsLatitude
        => DegreeToDms(Latitude) + (Latitude < 0.0 ? " S" : " N");

    /// <summary>
    /// The longitude as a DMS string
    /// </summary>
    public string DmsLongitude
        => DegreeToDms(Longitude) + (Longitude < 0.0 ? " W" : " E");
    
    public GlobePoint(double latitude = 0.0, double longitude = 0.0, double altitude = 0.0)
    {
        Latitude = LatitudeBounds.Clamp(latitude);
        Longitude = LongitudeBounds.Clamp(longitude);
        Altitude = altitude;
    }

    public GlobePoint WithAltitude(double newAltitude)
        => new(Latitude, Longitude, newAltitude);
    
    /// <summary>
    /// Converts a string to a GlobePoint, expecting the format (lat, lng, alt)
    /// </summary>
    /// <param name="s">the input string</param>
    /// <returns>A GlobePoint based on the string input</returns>
    public static explicit operator GlobePoint(string s)
    {
        var match = Regex.Match(s, CastPattern);
        if (!match.Success) throw new FormatException();
        return new GlobePoint(double.Parse(match.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture), 
            double.Parse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture),
            match.Groups[4].Value != "" ? double.Parse(match.Groups[4].Value, NumberStyles.Any, CultureInfo.InvariantCulture)
                : 0);
    }

    /// <summary>
    /// Calculates the DMS representation of the given degree value.
    /// </summary>
    /// <param name="degreeValue">given degree value</param>
    /// <returns>string representation of degree value in DMS</returns>
    static string DegreeToDms(double degreeValue)
    {
        degreeValue = Math.Abs(degreeValue);
        int num = (int)Math.Floor(degreeValue);
        int num2 = (int)Math.Floor((degreeValue - num) * 60.0);
        int num3 = (int)Math.Round(((degreeValue - num) * 60.0 - num2) * 60.0);
        return $"{num}° {num2,2}' {num3,2}''";
    }
    
    public override string ToString()
        => $"{Latitude.ToString(CultureInfo.InvariantCulture)},{Longitude.ToString(CultureInfo.InvariantCulture)}";

    public static GlobePoint operator +(GlobePoint a, GlobePoint b)
        => new(a.Latitude + b.Latitude, a.Longitude + b.Longitude, a.Altitude + b.Altitude);
    public static GlobePoint operator -(GlobePoint a, GlobePoint b)
        => new(a.Latitude - b.Latitude, a.Longitude - b.Longitude, a.Altitude - b.Altitude);
    public static GlobePoint operator /(GlobePoint a, double value)
        => new(a.Latitude / value, a.Longitude / value, a.Altitude / value);
}