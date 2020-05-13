public class POrderMoveToEnemy : POrderMoveTo
{
  public POrderMoveToEnemy(Pac owner,  GameField field, Point target) : base(owner, field, target)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return base.IsCompleted(cx) || !cx.Field.Grid[_target.Y, _target.X].HasFlag(CellFlags.EnemyPac);
  }
}

public class POrderMoveToPellet : POrderMoveTo
{
  public POrderMoveToPellet(Pac owner, GameField field, Point target) : base(owner, field, target)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return base.IsCompleted(cx) || !cx.Field.Grid[_target.Y, _target.X].HasFlag(CellFlags.HadPellet);
  }
}
public class POrderMoveToDiscovery : POrderMoveTo
{
  public POrderMoveToDiscovery(Pac owner,  GameField field, Point target) : base(owner, field, target)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return base.IsCompleted(cx) || cx.Field.Grid[_target.Y, _target.X].HasFlag(CellFlags.Seen);
  }
}