using System;

[Serializable]
public struct Cell
{
  public Point Pos;
  public CellFlags Flags;
  public int? PelletCount;

  public bool IsBlocked => HasFlag(CellFlags.Wall);
                           // || HasFlag(CellFlags.EnemyPac)
                           // || HasFlag(CellFlags.MyPac);

  public bool SetPellet(int count)
  {
    if (PelletCount.HasValue && PelletCount == count)
    {
      if (count > 0)
        SetFlag(CellFlags.Pellet);
      return false;
    }

    if (count > 0)
    {
      SetFlag(CellFlags.Pellet | CellFlags.HadPellet);

      if (count >= 10)
        SetFlag(CellFlags.GemPellet);
    }
    else
    {
      if (PelletCount >= 10) ResetFlag(CellFlags.GemPellet);
      if (Flags.CHasFlag(CellFlags.HadPellet)) ResetFlag(CellFlags.HadPellet);
    }

    PelletCount = count;
    return true;
  }

  public void ResetTick()
  {
    ResetFlag(CellFlags.Pellet);
    ResetFlag(CellFlags.MyPac);
    ResetFlag(CellFlags.EnemyPac);
    ResetFlag(CellFlags.Visible);
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