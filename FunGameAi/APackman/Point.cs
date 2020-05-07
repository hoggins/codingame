internal struct Point
{
  public ushort X, Y;

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

  public override string ToString()
  {
    return $"<{X} {Y}>";
  }
}