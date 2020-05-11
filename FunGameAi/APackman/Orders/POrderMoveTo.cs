using System.Linq;

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

public class POrderMoveByPath : POrderBase
{
  protected readonly Path _path;
  private Point? _lastPos;
  private bool _isBlocked;

  public POrderMoveByPath(Pac owner, Path path) : base(owner)
  {
    // Player.Print($"new {owner} to {path}");
    _path = path;
  }

  public override bool IsCompleted(Context cx)
  {
    // Player.Print($"che {Owner} to {_path}");
    return _isBlocked || Owner.Pos == _path.Last();
  }

  public override bool Execute(Context cx)
  {
    Player.Print($"block ? last:{_lastPos} cur:{Owner.Pos} {_lastPos == Owner.Pos}");
    if (_lastPos.HasValue && _lastPos.Value == Owner.Pos)
    {
      _isBlocked = true;
      return false;
    }

    _lastPos = Owner.Pos;

    var speed = Owner.IsBoosted ? 2 : 1;
    var nextPoint = _path.NextPoint(Owner.Pos, speed);

    Owner.Move(nextPoint, _path.Last().ToString());
    return true;
  }
}