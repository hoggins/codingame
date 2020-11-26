public struct Point
{
  public readonly ushort X;
  public readonly ushort Y;

  public Point(int x, int y)
  {
    X = (ushort) x;
    Y = (ushort) y;
  }

  public bool Equals(Point other)
  {
    return X == other.X && Y == other.Y;
  }

  public override bool Equals(object obj)
  {
    return obj is Point other && Equals(other);
  }

  public override int GetHashCode()
  {
    unchecked
    {
      return (X << 16) + Y;
    }
  }

  public static bool operator ==(Point p1, Point p2)
  {
    return p1.Equals(p2);
  }

  public static bool operator !=(Point p1, Point p2)
  {
    return !(p1 == p2);
  }

  public void Deconstruct(out int x, out int y)
  {
    x = X;
    y = Y;
  }
}