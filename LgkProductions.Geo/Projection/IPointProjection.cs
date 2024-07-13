namespace LgkProductions.Geo.Projection;

/// <summary>
/// Projects points from <see cref="GlobePoint"/>s (lla) to positions on a flat plane.
/// Also provides a method for getting the factor by what objects at a given <see cref="GlobePoint"/>
/// have to be scaled to match the surroundings, because most projections (most notably the Mercator-Projection)
/// do not sustain scale.
/// </summary>
public interface IPointProjection
{
    /// <summary>
    /// Projects a <see cref="GlobePoint"/> onto a flat plane.
    /// The altitude is the new y-Coordinate and will be scaled, based on the given <see cref="GlobePoint"/>.
    /// </summary>
    /// <param name="globePoint">the <see cref="GlobePoint"/> to project</param>
    /// <returns>A position as a <see cref="WorldPosition"/> with the altitude as the y-Coordinate</returns>
    public WorldPosition GlobePointToWorldPosition(GlobePoint globePoint);

    /// <summary>
    /// Calculates the <see cref="GlobePoint"/> matching the given position. This assumes that the Altitude is scaled
    /// based on the <see cref="GlobePoint"/>.
    /// </summary>
    /// <param name="position">The position to calculate the <see cref="GlobePoint"/> for as a <see cref="WorldPosition"/></param>
    /// <returns>A <see cref="GlobePoint"/> matching the given position</returns>
    public GlobePoint WorldPositionToGlobePoint(WorldPosition position);

    /// <summary>
    /// Calculates the pixel a given <see cref="GlobePoint"/> is in, when looking at a given <see cref="GlobeArea"/>
    /// </summary>
    /// <param name="area">The <see cref="GlobeArea"/> too look at</param>
    /// <param name="globePoint">The GlobePoint to calculate the pixel for</param>
    /// <param name="width">The amount of pixels in a row</param>
    /// <param name="height">The amount of pixels in a column</param>
    /// <returns>The pixel coordinates for the pixel, the GlobePoint is in</returns>
    /// <exception cref="ArgumentException">Thrown, if the area does not contain the point</exception>
    public (int x, int y) GlobePointToPixel(GlobeArea area, GlobePoint globePoint, uint width, uint height)
    {
        if (!area.Contains(globePoint))
            throw new ArgumentException("Area must contain point");

        var maxPoint = GlobePointToWorldPosition(new GlobePoint(area.BoundsLat.Max, area.BoundsLon.Max));
        var minPoint = GlobePointToWorldPosition(new GlobePoint(area.BoundsLat.Min, area.BoundsLon.Min));
        var worldBounds = maxPoint - minPoint;

        var point = GlobePointToWorldPosition(globePoint);

        var x = (int)((point.X - minPoint.X) / worldBounds.X * (width - 1));
        var y = (int)((height - 1) - (point.Z - minPoint.Z) / worldBounds.Z * (height - 1));
        return (x, y);
    }

    /// <summary>
    /// Calculates the factor, by which an object would need to get scaled, to match the surroundings.
    /// This is used for non-scale sustaining projections (e.g. the Mercator-Projection).
    /// </summary>
    /// <param name="globePoint">The <see cref="GlobePoint"/> to calculate the scale factor at</param>
    /// <returns>A scale factor as a double. If the argument given to this function is null, it returns 1</returns>
    public double GetScaleFactor(GlobePoint? globePoint);
}