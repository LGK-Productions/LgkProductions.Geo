using System.Text;

namespace LgkProductions.Geo;

public readonly record struct TileId(TileCoordinate Coordinates, int Zoom)
{
    private const char Separator = '_';
    
    public TileId(int x, int y, int zoom) : this(new TileCoordinate(x, y), zoom)
    {
    }

    /// <summary>
    /// Calculates the neighbouring TileId
    /// </summary>
    /// <param name="direction">The direction to calculate the neighbour of</param>
    /// <returns>The neighbouring TileId</returns>
    public TileId GetNeighbour(TileCoordinate direction)
    {
        return new TileId(Coordinates + direction, Zoom);
    }

    /// <summary>
    /// Calculates top left sub-tile id of the current tile
    /// </summary>
    /// <param name="zoomDifference">The zoom difference</param>
    /// <returns>The <see cref="TileId"/> of the top left sub-tile</returns>
    public TileId GetSubTile(int zoomDifference = 1)
    {
        return new TileId(Coordinates << zoomDifference, Zoom + zoomDifference);
    }
    
    /// <summary>
    /// Returns all sub-tiles of the current tile with the given zoom difference
    /// </summary>
    /// <param name="zoomDifference">The amount of sub-tile iterations to perform</param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing all sub-tiles</returns>
    public IEnumerable<TileId> GetSubTiles(int zoomDifference = 1)
    {
        var baseCoords = GetSubTile(zoomDifference).Coordinates;
        
        for (var i = 0; i < 1 << zoomDifference; i++)
        {
            for (var j = 0; j < 1 << zoomDifference; j++)
            {
                yield return new TileId(baseCoords + new TileCoordinate(i, j), Zoom + zoomDifference);
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
        zoomDifference = Math.Abs(zoomDifference);
        return new TileId(Coordinates >> zoomDifference, Zoom - zoomDifference);
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

        return tileId.Coordinates == Coordinates >> Zoom - tileId.Zoom;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Zoom}{Separator}{Coordinates.X}{Separator}{Coordinates.Y}";
    }

    /// <summary>
    /// Creates a new TileId from a given string of the format {Zoom}{Separator}{X}{Separator}{Y}
    /// </summary>
    /// <param name="key">A string of the format {Zoom}{Separator}{X}{Separator}{Y}</param>
    /// <returns>A TileId based on the given string</returns>
    /// <exception cref="ArgumentException">Thrown, if the string is not in expected format</exception>
    public static TileId FromString(string key)
    {
            var values = key.Split(Separator);
            if (values.Length != 3)
                throw new ArgumentException(
                    $"TileId key has to contain 3 values, but consists of {values.Length} values");
            return new TileId(int.Parse(values[1]), int.Parse(values[2]), int.Parse(values[0]));
    }
    
    /// <summary>
    /// Calculates the matching quadkey for the this tile
    /// </summary>
    /// <returns>The quadkey as a string</returns>
    /// <exception cref="ArgumentException">thrown, if the zoom is 0 (which is not possible to convert)</exception>
    public string ToQuadKey()
    {
        if (Zoom == 0)
            throw new ArgumentException("Cannot convert tile with zoom 0 to quadkey");
        StringBuilder builder = new();
        TileId previousTile = new();
        for (int i = 1; i <= Zoom; i++)
        {
            var current = GetParentTile(Zoom - i);
            var relativeCoords = current.Coordinates - previousTile.GetSubTile().Coordinates;
            builder.Append(relativeCoords.X + 2 * relativeCoords.Y);
            previousTile = current;
        }
        return builder.ToString();
    }

    /// <summary>
    /// Checks if the tile coordinates are in bounds
    /// </summary>
    /// <returns><c>true</c>, if the tile coordinates are in bounds, <c>false</c> otherwise</returns>
    public bool IsInbounds()
    {
        var maxCoord = 1 << Zoom;
        return Coordinates.X < maxCoord && Coordinates.Y < maxCoord && Coordinates.X >= 0 && Coordinates.Y >= 0;
    }
}