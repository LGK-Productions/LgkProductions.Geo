using System.Text.Json.Serialization;

namespace LgkProductions.Geo.Projection;

/// <summary>
/// Projects <see cref="GlobePoint"/>/<see cref="GlobeArea"/> to <see cref="TileId"/> and vice versa. 
/// </summary>
[JsonDerivedType(typeof(WebMercatorProjection), "WebMercatorProjection")]
public interface ITileProjection
{
    /// <summary>
    /// Calculates the absolute tile coordinates of the tile containing the given <see cref="GlobePoint"/> based on
    /// the given Zoom-Factor.
    /// </summary>
    /// <param name="globePoint">The <see cref="GlobePoint"/> to calculate the absolute tile coordinates for</param>
    /// <param name="zoomFactor">The Zoom-Factor to get the absolute tile coordinates for</param>
    /// <param name="clamp">Whether the <paramref name="globePoint"/> should be clamped to a matching value range</param>
    /// <returns>The absolute tile coordinates of the tile containing the given <see cref="GlobePoint"/></returns>
    public TileCoordinate GlobePointToTileCoordinates(GlobePoint globePoint, int zoomFactor, bool clamp = true);

    /// <summary>
    /// Calculates the <see cref="GlobePoint"/> at the north-west corner of the tile with the given absolute tile coordinates
    /// and the given Zoom-Factor.
    /// </summary>
    /// <param name="tile">The tile to calculate the north-west corner point for</param>
    /// <returns>A <see cref="GlobePoint"/> at the north-west corner of the tile</returns>
    public GlobePoint TileToGlobePoint(TileId tile);

    /// <summary>
    /// Converts the given tile to a <see cref="GlobeArea"/>
    /// </summary>
    /// <param name="tile">The tile to convert</param>
    /// <returns>A <see cref="GlobeArea"/> matching the given tile coordinates and zoom factor</returns>
    public GlobeArea TileToGlobeArea(TileId tile)
    {
        return new GlobeArea(TileToGlobePoint(tile),
            TileToGlobePoint(tile with { Coordinates = tile.Coordinates + TileCoordinate.One }));
    }

    /// <summary>
    /// Converts the given <see cref="GlobeArea"/> to multiple tiles with the given zoom factor, covering the whole area
    /// </summary>
    /// <param name="globeArea">The <see cref="GlobeArea"/> to convert</param>
    /// <param name="zoomFactor">The zoom factor of the resulting tiles</param>
    /// <returns>An IEnumerable of tiles covering the given <see cref="GlobeArea"/></returns>
    public IEnumerable<TileId> GlobeAreaToTiles(GlobeArea globeArea, int zoomFactor)
    {
        var ne = GlobePointToTileCoordinates(globeArea.NorthEastCorner, zoomFactor);
        var sw = GlobePointToTileCoordinates(globeArea.SouthWestCorner, zoomFactor);
        return EnumerateTiles(ne, sw, zoomFactor);
    }

    private IEnumerable<TileId> EnumerateTiles(TileCoordinate corner1, TileCoordinate corner2, int zoomFactor)
    {
        var boundsX = new Bounds<int>(corner1.X, corner2.X);
        var boundsY = new Bounds<int>(corner1.Y, corner2.Y);
        for (var x = boundsX.Min; x <= boundsX.Max; x++)
        {
            for (var y = boundsY.Min; y <= boundsY.Max; y++)
            {
                yield return new TileId(new(x, y), zoomFactor);
            }
        }
    }

    /// <summary>
    /// Calculates a number of tiles covering the whole GlobeArea. Chooses a zoom factor based on the targetTileCount.
    /// This returns the target tile count or less tiles if possible.
    /// </summary>
    /// <param name="globeArea">The <see cref="GlobeArea"/> to convert</param>
    /// <param name="zoomBounds">The bounds of the zoom factor</param>
    /// <param name="targetTileCount">The amount of tiles that should be targeted for returning.</param>
    /// <returns>An IEnumerable of tiles covering the given <see cref="GlobeArea"/></returns>
    public IEnumerable<TileId> GlobeAreaToTiles(GlobeArea globeArea, Bounds<int> zoomBounds, int targetTileCount)
    {
        var currentZoom = zoomBounds.Min;
        while (true)
        {
            var ne = GlobePointToTileCoordinates(globeArea.NorthEastCorner, currentZoom);
            var sw = GlobePointToTileCoordinates(globeArea.SouthWestCorner, currentZoom);
            
            var xBounds = new Bounds<int>(ne.X, sw.X);
            var yBounds = new Bounds<int>(ne.Y, sw.Y);

            var tileCount = (xBounds.Size + 1) * (yBounds.Size + 1);
            if (tileCount == targetTileCount)
                return EnumerateTiles(ne, sw, currentZoom);
            else if (tileCount > targetTileCount)
                return GlobeAreaToTiles(globeArea, Math.Clamp(currentZoom - 1, zoomBounds.Min, zoomBounds.Max));

            currentZoom++;

            if (currentZoom > zoomBounds.Max)
            {
                return GlobeAreaToTiles(globeArea, currentZoom - 1);
            }
        }
    }
}