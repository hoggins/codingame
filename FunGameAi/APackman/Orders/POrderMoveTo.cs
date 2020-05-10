public abstract class POrderMoveTo : POrderBase
{
  protected readonly Point _target;
  private Point? _lastPos;
  private bool _isBlocked;

  public POrderMoveTo(Pac owner, Point target) : base(owner)
  {
    Player.Print($"new {owner} to {target}");
    _target = target;
  }

  public override bool IsCompleted(Context cx)
  {
    Player.Print($"che {Owner} to {_target}");
    return _isBlocked || Owner.Pos == _target;
  }

  public override bool Execute(Context cx)
  {
    if (_lastPos.HasValue && _lastPos.Value == Owner.Pos)
    {
      _isBlocked = true;
      return false;
    }

    _lastPos = Owner.Pos;
    Owner.Move(_target, _target.ToString());
    return true;
  }
}