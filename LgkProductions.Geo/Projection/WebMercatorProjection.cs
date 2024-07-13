namespace LgkProductions.Geo.Projection;

/// <summary>
/// <para>
/// An Implementation of the spherical Web-Mercator-Projection based on Google's implementation (EPSG:900913),
/// as it is the standard for most map tile services.
/// This is based on the explanations found at
/// https://www.maptiler.com/google-maps-coordinates-tile-bounds-projection
/// </para>
/// <para>
/// Calculating the tile coordinates is based on
/// https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
/// </para>
/// </summary>
public sealed class WebMercatorProjection : IProjection
{
    /// <summary>
    /// The Earth's radius in metres
    /// </summary>
    private const int EarthRadius = 6378137;

    /// <summary>
    /// Shifts the origin by half the Earth's circumference
    /// </summary>
    private const double OriginShift = 2 * Math.PI * EarthRadius / 2.0;

    /// <summary>
    /// Converts a given <see cref="GlobePoint"/> in WGS84 to a XZ in Spherical Mercator EPSG:900913 with a scaled Y-height,
    /// based on the given points position.
    /// </summary>
    /// <param name="globePoint">The <see cref="GlobePoint"/> to be converted</param>
    /// <returns>A position with XZ point in Spherical Mercator EPSG:900913 and a scaled Y-Height as a <see cref="WorldPosition"/></returns>
    public WorldPosition GlobePointToWorldPosition(GlobePoint globePoint)
    {
        // project lat, lon to x, z - plane
        var position = new WorldPosition()
        {
            X = globePoint.Longitude * OriginShift / 180.0,
            Y = globePoint.Altitude * GetScaleFactor(globePoint), //scale altitude according to position
            Z = Math.Log(Math.Tan((90 + globePoint.Latitude) * Math.PI / 360.0)) / (Math.PI / 180.0) * OriginShift /
                180.0
        };
        return position;
    }

    /// <summary>
    /// Converts a given position with a XZ point in Spherical Mercator EPSG:900913 to a <see cref="GlobePoint"/>
    /// in WGS84 with the Y-Height scaled accordingly to position.
    /// </summary>
    /// <param name="position">A position with a XZ point in Spherical Mercator EPSG:900913</param>
    /// <returns>A <see cref="GlobePoint"/> in WGS84 with the Y-Height scaled accordingly to position</returns>
    public GlobePoint WorldPositionToGlobePoint(WorldPosition position)
    {
        var lat = 180 / Math.PI
                  * (2 * Math.Atan(Math.Exp((position.Z / OriginShift * 180.0) * Math.PI / 180.0)) - Math.PI / 2.0);

        //scale altitude down
        var globePoint = new GlobePoint(lat, position.X / OriginShift * 180.0);

        return globePoint.WithAltitude(position.Y / GetScaleFactor(globePoint));
    }

    /// <summary>
    /// Calculates the absolute tile coordinates of the tile containing the given <see cref="GlobePoint"/> based on
    /// the given Zoom-Factor.
    /// </summary>
    /// <param name="globePoint">The <see cref="GlobePoint"/> to calculate the absolute tile coordinates for</param>
    /// <param name="zoomFactor">The Zoom-Factor to get the absolute tile coordinates for</param>
    /// <returns>The absolute tile coordinates of the tile containing the given <see cref="GlobePoint"/></returns>
    public TileCoordinate GlobePointToTileCoordinates(GlobePoint globePoint, int zoomFactor)
    {
        var latRad = globePoint.Latitude / 180 * Math.PI;
        return new TileCoordinate()
        {
            //Longitude to tileX
            X = (int)Math.Floor((globePoint.Longitude + 180) / 360 * (1 << zoomFactor)),
            //Latitude to tileY
            Y = (int)Math.Floor((1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI)
                / 2 * (1 << zoomFactor))
        };
    }

    /// <summary>
    /// Calculates the <see cref="GlobePoint"/> at the north-west corner of the tile with the given absolute tile coordinates
    /// and the given Zoom-Factor.
    /// </summary>
    /// <param name="tile">The tile to convert</param>
    /// <returns>A <see cref="GlobePoint"/> at the north-west corner of the tile</returns>
    public GlobePoint TileToGlobePoint(TileId tile)
    {
        var tmp = Math.PI - 2.0 * Math.PI * tile.Coordinates.Y / (1 << tile.Zoom);

        return new GlobePoint(
            180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(tmp) - Math.Exp(-tmp))),
            tile.Coordinates.X / (double)(1 << tile.Zoom) * 360 - 180);
    }

    /// <summary>
    /// Calculates the factor, by which an object would need to get scaled, to match the surroundings.
    /// This is because the Mercator-Projection is non-scale sustaining.
    /// </summary>
    /// <param name="globePoint">The <see cref="GlobePoint"/> to calculate the scale factor at</param>
    /// <returns>A scale factor as a double. If the argument given to this function is null, it returns 1</returns>
    public double GetScaleFactor(GlobePoint? globePoint)
    {
        if (globePoint == null)
        {
            return 1;
        }

        return 1 / Math.Cos(globePoint.Value.Latitude * (Math.PI / 180.0));
    }
}