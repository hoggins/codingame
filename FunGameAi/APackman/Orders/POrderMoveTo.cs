public abstract class POrderMoveTo : POrderMoveByPath
{
  protected readonly Point _target;
  public POrderMoveTo(Pac owner, GameField field, Point target) : base(owner, field, target)
  {
    _target = target;
  }
}