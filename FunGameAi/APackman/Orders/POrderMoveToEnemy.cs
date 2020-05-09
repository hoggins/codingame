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