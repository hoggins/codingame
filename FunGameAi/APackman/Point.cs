using System;

public struct Point
{
  public readonly ushort X, Y;

  public Point(int x, int y)
  {
    X = (ushort) x;
    Y = (ushort) y;
  }

  public Point(ushort x, ushort y)
  {
    X = x;
    Y = y;
  }

  public int Distance(Point other)
  {
    return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
  }


  public override string ToString()
  {
    return $"<{X} {Y}>";
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

  public int ToIdx(int rowLen)
  {
    return Y * rowLen + X;
  }
}