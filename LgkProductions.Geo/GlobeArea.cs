namespace LgkProductions.Geo;

/// <summary>
/// A data class storing an rectangular area on the earth.
/// </summary>
public readonly record struct GlobeArea
{
    /// <summary>
    /// latitude bounds of the area in degrees with x &lt;= y
    /// </summary>
    public Bounds<double> BoundsLat { get; }

    /// <summary>
    /// longitude bounds of the area in degrees with x &gt;= y
    /// </summary>
    public Bounds<double> BoundsLon { get; }

    /// <summary>
    /// Height of the rectangular area in degrees
    /// </summary>
    private double AreaHeight => BoundsLat.Max - BoundsLat.Min;

    /// <summary>
    /// Width of the rectangular area in degrees
    /// </summary>
    private double AreaWidth => BoundsLon.Max - BoundsLon.Min;

    /// <summary>
    /// <see cref="GlobePoint"/> in the middle of the rectangular area on earth
    /// </summary>
    public GlobePoint MidPoint { get; }

    public GlobePoint NorthEastPoint => new(BoundsLat.Max, BoundsLon.Max);
    public GlobePoint NorthWestPoint => new(BoundsLat.Max, BoundsLon.Min);
    public GlobePoint SouthEastPoint => new(BoundsLat.Min, BoundsLon.Max);
    public GlobePoint SouthWestPoint => new(BoundsLat.Min, BoundsLon.Min);

    /// <summary>
    /// Creates a new <see cref="GlobeArea"/> with the <see cref="GlobePoint"/> in the north-west,
    /// and the <see cref="GlobePoint"/> in the south-east of the target area
    /// </summary>
    /// <param name="northWestPoint">A <see cref="GlobePoint"/> at the north-west corner of the target area</param>
    /// <param name="southEastPoint">A <see cref="GlobePoint"/> at the south-east corner of the target area</param>
    public GlobeArea(GlobePoint northWestPoint, GlobePoint southEastPoint)
    {
        BoundsLat = new Bounds<double>(southEastPoint.Latitude, northWestPoint.Latitude);
        BoundsLon = new Bounds<double>(northWestPoint.Longitude, southEastPoint.Longitude);

        //get Midpoint
        var lat = BoundsLat.Min + AreaHeight / 2;
        var lon = BoundsLon.Min + AreaWidth / 2;
        MidPoint = new GlobePoint(lat, lon);
    }

    /// <summary>
    /// Gets a List of GlobePoints equally spread over the Area
    /// </summary>
    /// <param name="resolution">the amount of points on each axis</param>
    /// <returns>a resolution x resolution grid of GlobePoints</returns>
    public IReadOnlyList<GlobePoint> GetPointGrid(int resolution)
    {
        List<GlobePoint> pointGrid = [];
        for (var i = 0; i < resolution; i++)
        {
            for (var j = 0; j < resolution; j++)
            {
                pointGrid.Add(new GlobePoint(BoundsLat.Min + i * (AreaHeight / (resolution - 1)),
                    BoundsLon.Min + j * (AreaWidth / (resolution - 1))));
            }
        }

        return pointGrid;
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
    /// Returns the <see cref="GlobePoint"/> in the GlobeArea, nearest to the given <see cref="GlobePoint"/>
    /// </summary>
    /// <param name="globePoint">The <see cref="GlobePoint"/> to check</param>
    /// <returns>The <see cref="GlobePoint"/> in the GlobeArea, nearest to the given <see cref="GlobePoint"/></returns>
    public GlobePoint GetClosestPoint(GlobePoint globePoint)
    {
        var lat = Math.Clamp(globePoint.Latitude, BoundsLat.Min, BoundsLat.Max);
        var lon = Math.Clamp(globePoint.Longitude, BoundsLon.Min, BoundsLon.Max);

        return new GlobePoint(lat, lon);
    }
}