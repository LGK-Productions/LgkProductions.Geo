namespace LgkProductions.Geo;

public readonly record struct WorldPosition(double X, double Y, double Z)
{
    public static WorldPosition operator +(WorldPosition left, WorldPosition right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

    public static WorldPosition operator -(WorldPosition left, WorldPosition right)
        => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
}