using System.Globalization;
using System.Text.RegularExpressions;

namespace LgkProductions.Geo;

/// <summary>
/// A data class storing an rectangular area on the earth.
/// </summary>
public readonly record struct GlobeArea
{
    private const string CastPattern = @"\s*\(\s*(\(.+\))\s*,\s*(\(.+\))\s*\)\s*";
    
    /// <summary>
    /// latitude bounds of the area in degrees with x &lt;= y
    /// </summary>
    public Bounds<double> BoundsLat { get; init; }

    /// <summary>
    /// longitude bounds of the area in degrees with x &gt;= y
    /// </summary>
    public Bounds<double> BoundsLon { get; init; }

    /// <summary>
    /// <see cref="GlobePoint"/> in the middle of the rectangular area on earth
    /// </summary>
    public GlobePoint MidPoint { get; }

    public GlobePoint NorthEastCorner => new(BoundsLat.Max, BoundsLon.Max);
    public GlobePoint NorthWestCorner => new(BoundsLat.Max, BoundsLon.Min);
    public GlobePoint SouthEastCorner => new(BoundsLat.Min, BoundsLon.Max);
    public GlobePoint SouthWestCorner => new(BoundsLat.Min, BoundsLon.Min);
    
    public IEnumerable<GlobePoint> Corners => new[] { NorthEastCorner, NorthWestCorner, SouthEastCorner, SouthWestCorner };

    /// <summary>
    /// Creates a new <see cref="GlobeArea"/> with the <see cref="GlobePoint"/> in the north-west,
    /// and the <see cref="GlobePoint"/> in the south-east of the target area
    /// </summary>
    /// <param name="corner1">A <see cref="GlobePoint"/> at a corner of the target area</param>
    /// <param name="corner2">A <see cref="GlobePoint"/> at a corner of the target area</param>
    public GlobeArea(GlobePoint corner1, GlobePoint corner2)
    {
        BoundsLat = new Bounds<double>(corner1.Latitude, corner2.Latitude);
        BoundsLon = new Bounds<double>(corner1.Longitude, corner2.Longitude);

        //get Midpoint
        var lat = BoundsLat.Min + BoundsLat.Size / 2;
        var lon = BoundsLon.Min + BoundsLon.Size / 2;
        MidPoint = new GlobePoint(lat, lon);
    }
    
    /// <summary>
    /// Converts a string to a GlobeArea, expecting the format (GlobePoint, GlobePoint)
    /// </summary>
    /// <param name="s">the input string</param>
    /// <returns>A GlobeArea based on the string input</returns>
    public static GlobeArea Parse(string s)
    {
        var match = Regex.Match(s, CastPattern);
        if (!match.Success) throw new FormatException();
        return new GlobeArea(GlobePoint.Parse(match.Groups[1].Value), GlobePoint.Parse(match.Groups[2].Value));
    }

    /// <summary>
    /// Gets a List of GlobePoints equally spread over the Area
    /// </summary>
    /// <param name="resolution">the amount of points on each axis</param>
    /// <returns>a resolution x resolution grid of GlobePoints</returns>
    public IEnumerable<GlobePoint> GetPointGrid(int resolution)
    {
        for (var i = 0; i < resolution; i++)
        {
            for (var j = 0; j < resolution; j++)
            {
                yield return new GlobePoint(BoundsLat.Min + i * (BoundsLat.Size / (resolution - 1)),
                    BoundsLon.Min + j * (BoundsLon.Size / (resolution - 1)));
            }
        }
    }

    /// <summary>
    /// Checks whether this <see cref="GlobeArea"/> contains a given <see cref="GlobePoint"/>
    /// </summary>
    /// <param name="globePoint">The <see cref="GlobePoint"/> to check</param>
    /// <returns><c>true</c>, if the <see cref="GlobeArea"/> contains the <see cref="GlobePoint"/>, <c>false</c> otherwise</returns>
    public bool Contains(GlobePoint globePoint)
    {
        return BoundsLat.Contains(globePoint.Latitude) && BoundsLon.Contains(globePoint.Longitude);
    }

    /// <summary>
    /// Checks whether this <see cref="GlobeArea"/> contains a given <see cref="GlobeArea"/>
    /// </summary>
    /// <param name="globeArea">The <see cref="GlobeArea"/> to check</param>
    /// <returns><c>true</c>, if the <see cref="GlobeArea"/> contains the <see cref="GlobeArea"/>, <c>false</c> otherwise</returns>
    public bool Contains(GlobeArea globeArea)
    {
        return BoundsLat.Contains(globeArea.BoundsLat) && BoundsLon.Contains(globeArea.BoundsLon);
    }

    /// <summary>
    /// Checks whether this <see cref="GlobeArea"/> intersects with the given <see cref="GlobeArea"/>
    /// </summary>
    /// <param name="other">The <see cref="GlobeArea"/> to check</param>
    /// <returns><c>true</c>, if the <see cref="GlobeArea"/> intersects with the <see cref="GlobeArea"/>, <c>false</c> otherwise</returns>
    public bool Intersects(GlobeArea other)
    {
        return BoundsLat.Overlaps(other.BoundsLat) && BoundsLon.Overlaps(other.BoundsLon);
    }

    /// <summary>
    /// Returns the <see cref="GlobePoint"/> in the GlobeArea, nearest to the given <see cref="GlobePoint"/>, when projected
    /// onto a flat surface
    /// </summary>
    /// <param name="globePoint">The <see cref="GlobePoint"/> to check</param>
    /// <returns>The <see cref="GlobePoint"/> in the GlobeArea, nearest to the given <see cref="GlobePoint"/></returns>
    public GlobePoint GetClosestPoint(GlobePoint globePoint)
    {
        var lat = BoundsLat.Clamp(globePoint.Latitude);
        var lon = BoundsLon.Clamp(globePoint.Longitude);

        return new GlobePoint(lat, lon);
    }
}