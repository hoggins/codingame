internal struct Cell
{
  public Point Pos;
  public CellFlags Flags;
  public int PelletCount;

  public bool IsBlocked => Flags.HasFlag(CellFlags.Wall) || Flags.HasFlag(CellFlags.EnemyPac);

  public void SetPellet(int count)
  {
    if (count > 0)
      SetFlag(CellFlags.Pellet);
    else
      ResetFlag(CellFlags.Pellet);
    PelletCount = count;
  }

  public void ResetTick()
  {
    ResetFlag(CellFlags.Pellet | CellFlags.MyPac | CellFlags.EnemyPac);
  }

  private void ResetFlag(CellFlags flag)
  {
    Flags &= ~flag;
  }

  public void SetFlag(CellFlags flag)
  {
    Flags |= flag;
  }

  public override string ToString()
  {
    return Flags.ToString();
  }
}