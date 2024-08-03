using System.Numerics;

namespace LgkProductions.Geo;

public readonly record struct WorldPosition(double X, double Y, double Z)
{
    public static WorldPosition operator +(WorldPosition left, WorldPosition right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static WorldPosition operator -(WorldPosition left, WorldPosition right)
        => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    
    public static WorldPosition operator *(WorldPosition position, float factor)
        => new(position.X * factor, position.Y * factor, position.Z * factor);
    
    public static WorldPosition operator *(float factor, WorldPosition position)
        => new(position.X * factor, position.Y * factor, position.Z * factor);
    
    public static WorldPosition operator /(WorldPosition position, float divisor)
        => new(position.X / divisor, position.Y / divisor, position.Z / divisor);

    public static WorldPosition Zero => new WorldPosition(0, 0, 0);
}