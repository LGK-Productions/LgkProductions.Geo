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
    /// The maximum latitude usable by tile calculation. GlobePoints might accept higher Latitudes, but tile calculations
    /// will not work when using values higher than the specified maximum. By default values will get clamped
    /// </summary>
    private const double MaxLatitude = 85.05112877980659;
    
    /// <summary>
    /// The maximum longitude usable by tile calculation. GlobePoints might accept higher longitudes, but tile calculations
    /// will not work when using values higher than the specified maximum. By default values will get clamped
    /// </summary>
    private const double MaxLongitude = 179.9998;

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

    /// <inheritdoc/>
    public TileCoordinate GlobePointToTileCoordinates(GlobePoint globePoint, int zoomFactor, bool clamp = true)
    {
        if (clamp)
        {
            globePoint = globePoint with
            {
                Longitude = Math.Clamp(globePoint.Longitude, -MaxLongitude, MaxLongitude),
                Latitude = Math.Clamp(globePoint.Latitude, -MaxLatitude, MaxLatitude)
            };
        }
        
        var latRad = globePoint.Latitude / 180 * Math.PI;
        var n = 1 << zoomFactor;
        return new TileCoordinate()
        {
            //Longitude to tileX
            X = (int)(n * ((globePoint.Longitude + 180) / 360)),
            //Latitude to tileY
            Y = (int)(n * (1 - Math.Log(Math.Tan(latRad) + (1 / Math.Cos(latRad))) / Math.PI)) / 2
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
        var n = 1 << tile.Zoom;
        var latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * tile.Coordinates.Y / (double)n)));
        return new GlobePoint(
            latRad * 180 / Math.PI,
            (double)tile.Coordinates.X / n * 360 - 180);
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