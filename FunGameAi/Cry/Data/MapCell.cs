public class MapCell
{
  public Point Pos;
  public int? Ore;
  public bool Hole;

  public bool IsMined;
  public bool IsDigged => DigCount > 0;
  public int DigCount;

  public int DigLock;

  public int? InitialOre;

  public MapCell(Point pos)
  {
    Pos = pos;
  }

  public bool IsSafe()
  {
    var diggedOnlyByMe = !InitialOre.HasValue || (InitialOre.Value - DigCount == Ore.GetValueOrDefault());
    return !IsMined && (IsDigged || !Hole) && diggedOnlyByMe;
  }

  public void Set(int? ore, bool hole)
  {
    Ore = ore;
    Hole = hole;

    if (ore.HasValue && !hole && !InitialOre.HasValue)
      InitialOre = ore;
  }

  public void IncreaseDig()
  {
    ++DigCount;
  }

  public int Distance(Point point)
  {
    return Utils.Distance(Pos, point);
  }
}