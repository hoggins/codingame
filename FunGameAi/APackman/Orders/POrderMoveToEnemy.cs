public class POrderMoveToEnemy : POrderMoveTo
{
  public POrderMoveToEnemy(Pac owner, Point target) : base(owner, target)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return base.IsCompleted(cx) || !cx.Map.Grid[_target.Y, _target.X].HasFlag(CellFlags.EnemyPac);
  }
}

public class POrderMoveToPellet : POrderMoveTo
{
  public POrderMoveToPellet(Pac owner, Point target) : base(owner, target)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return base.IsCompleted(cx) || !cx.Map.Grid[_target.Y, _target.X].HasFlag(CellFlags.HadPellet);
  }
}
public class POrderMoveToDiscovery : POrderMoveTo
{
  public POrderMoveToDiscovery(Pac owner, Point target) : base(owner, target)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return base.IsCompleted(cx) || cx.Map.Grid[_target.Y, _target.X].HasFlag(CellFlags.Seen);
  }
}