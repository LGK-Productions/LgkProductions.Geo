namespace LgkProductions.Geo;

public readonly record struct TileId(TileCoordinate Coordinates, int Zoom)
{
    public TileId(int x, int y, int zoom) : this(new TileCoordinate(x, y), zoom)
    {
    }

    /// <summary>
    /// Calculates top left sub-tile id of the current tile
    /// </summary>
    /// <param name="zoomDifference">The zoom difference</param>
    /// <returns>The <see cref="TileId"/> of the top left sub-tile</returns>
    public TileId GetTopLeftSubTile(int zoomDifference = 1)
    {
        return new TileId(Coordinates * (int)Math.Pow(2, zoomDifference),
            Zoom + zoomDifference);
    }

    public TileId GetNeighbour(TileCoordinate direction)
    {
        return new TileId(Coordinates + direction, Zoom);
    }

    /// <summary>
    /// Returns all sub-tiles of the current tile with the given zoom difference
    /// </summary>
    /// <param name="zoomDifference">The amount of sub-tile iterations to perform</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all sub-tiles</returns>
    public IEnumerable<TileId> GetSubTiles(int zoomDifference = 1)
    {
        for (var i = 0; i < Math.Pow(2, zoomDifference); i++)
        {
            for (var j = 0; j < Math.Pow(2, zoomDifference); j++)
            {
                yield return new TileId(Coordinates * (int)Math.Pow(2, zoomDifference) + new TileCoordinate(i, j),
                    Zoom + zoomDifference);
            }
        }
    }

    /// <summary>
    /// Returns the parent tile with the given zoom difference
    /// </summary>
    /// <param name="zoomDifference">The amount of parent iterations to perform</param>
    /// <returns>The parent tile with the given zoom difference</returns>
    public TileId GetParentTile(int zoomDifference = 1)
    {
        if (zoomDifference == 0) return this;
        if (zoomDifference < 0) zoomDifference = -zoomDifference;
        return new TileId(Coordinates / (int)Math.Pow(2, zoomDifference), Zoom - zoomDifference);
    }

    /// <summary>
    /// Checks if the current tile is a neighbour of the given tile
    /// </summary>
    /// <param name="neighbour">The tile to check for</param>
    /// <param name="direction">The direction to the given tile. This tiles Coordinates + the direction gives given tile coordinates (adjusted to same zoom)</param>
    /// <returns><c>true</c>, if the current tile is a neighbour of the given tile, <c>false</c> otherwise</returns>
    public bool IsNeighbourOf(TileId neighbour, out TileCoordinate direction)
    {
        var zoomDifference = Zoom - neighbour.Zoom;
        if (zoomDifference < 0)
        {
            var isNeighbour = neighbour.IsNeighbourOf(this, out direction);
            direction = -direction;
            return isNeighbour;
        }

        var parent = GetParentTile(zoomDifference);
        direction = neighbour.Coordinates - parent.Coordinates;
        if (Math.Abs(direction.X) + Math.Abs(direction.Y) != 1) return false;
        return !GetNeighbour(direction).GetParentTile(zoomDifference).Equals(parent); //make sure tile is at edge
    }

    /// <summary>
    /// Checks if the current tile is covered by the given tile (or if the given tile is a parent of this tile)
    /// </summary>
    /// <param name="tileId">The tile to check for</param>
    /// <returns><c>true</c>, if the current tile is covered by the given tile, <c>false</c> otherwise</returns>
    public bool IsCoveredBy(TileId tileId)
    {
        if (tileId.Zoom > Zoom) //the other tile is smaller
        {
            return false;
        }

        return tileId.Coordinates == Coordinates / (int)Math.Pow(2, Zoom - tileId.Zoom);
    }

    public override string ToString()
    {
        return $"{Zoom}_{Coordinates.X}_{Coordinates.Y}";
    }

    /// <summary>
    /// Checks if the tile coordinates are in bounds
    /// </summary>
    /// <returns><c>true</c>, if the tile coordinates are in bounds, <c>false</c> otherwise</returns>
    public bool IsInbounds()
    {
        var maxCoord = Math.Pow(2, Zoom);
        return Coordinates.X < maxCoord && Coordinates.Y < maxCoord && Coordinates.X >= 0 && Coordinates.Y >= 0;
    }
}