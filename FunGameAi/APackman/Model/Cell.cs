public struct Cell
{
  public Point Pos;
  public CellFlags Flags;
  public int PelletCount;

  public bool IsBlocked => HasFlag(CellFlags.Wall)
                           || HasFlag(CellFlags.EnemyPac)
                           || HasFlag(CellFlags.MyPac);

  public void SetPellet(int count)
  {
    if (count > 0)
      SetFlag(CellFlags.Pellet | CellFlags.HadPellet);
    else
      ResetFlag(CellFlags.Pellet);
    PelletCount = count;
  }

  public void ResetTick()
  {
    ResetFlag(CellFlags.Pellet | CellFlags.MyPac | CellFlags.EnemyPac | CellFlags.Visible);
  }

  public void ResetFlag(CellFlags flag)
  {
    Flags &= ~flag;
  }

  public void SetFlag(CellFlags flag)
  {
    Flags |= flag;
  }

  public override string ToString()
  {
    return Pos.ToString(); //Flags.ToString();
  }

  public bool HasFlag(CellFlags f)
  {
    return f >= 0 && (Flags & f) == f || f < 0 && (Flags & f) == Flags;
  }
}