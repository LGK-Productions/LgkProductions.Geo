﻿using System.Globalization;

namespace LgkProductions.Geo;

public readonly record struct GlobePoint
{
    public double Latitude { get; }
    public double Longitude { get; }
    public double Altitude { get; }

    static readonly Bounds<double> LatitudeBounds = new(-90.0, 90.0);
    static readonly Bounds<double> LongitudeBounds = new(-180.0, 180.0);

    public string DmsLatitude
        => DegreeToDms(Latitude) + (Latitude < 0.0 ? " S" : " N");

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
}