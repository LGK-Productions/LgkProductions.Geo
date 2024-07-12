using System.Numerics;

namespace LgkProductions.Geo;

public readonly record struct TileCoordinate(int X, int Y)
{
    public readonly Vector2 AsVector()
        => new(X, Y);

    public static TileCoordinate operator +(TileCoordinate left, TileCoordinate right)
        => new(left.X + right.X, left.Y + right.Y);

    public static TileCoordinate operator -(TileCoordinate left, TileCoordinate right)
        => new(left.X - right.X, left.Y - right.Y);

    public static TileCoordinate operator -(TileCoordinate value)
        => new(-value.X, -value.Y);

    public static TileCoordinate operator *(TileCoordinate value, int factor)
        => new(value.X * factor, value.Y * factor);

    public static TileCoordinate operator *(int factor, TileCoordinate value)
        => new(value.X * factor, value.Y * factor);

    public static TileCoordinate operator /(TileCoordinate value, int factor)
        => new(value.X / factor, value.Y / factor);

    public static TileCoordinate One => new(1, 1);
    public static TileCoordinate Up => new(0, -1);
    public static TileCoordinate Left => new(-1, 0);
    public static TileCoordinate Right => new(1, 0);
    public static TileCoordinate Down => new(0, 1);
}