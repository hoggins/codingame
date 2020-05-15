public class POrderMoveToEnemy : POrderMoveTo
{
  public POrderMoveToEnemy(Pac owner,  GameField field, Point target) : base(owner, field, target)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return base.IsCompleted(cx) || !cx.Field.GetFlags(_target).CHasFlag(CellFlags.EnemyPac);
  }
}

public class POrderMoveToPellet : POrderMoveTo
{
  public POrderMoveToPellet(Pac owner, GameField field, Point target) : base(owner, field, target)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return base.IsCompleted(cx) || !cx.Field.GetFlags(_target).CHasFlag(CellFlags.HadPellet);
  }
}
public class POrderMoveToDiscovery : POrderMoveTo
{
  public POrderMoveToDiscovery(Pac owner,  GameField field, Point target) : base(owner, field, target)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return base.IsCompleted(cx) || cx.Field.GetFlags(_target).CHasFlag(CellFlags.Seen);
  }
}