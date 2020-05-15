using System.Collections.Generic;
using System.Linq;

public class POrderMoveByBestPath : POrderMoveByPath
{
  public POrderMoveByBestPath(Pac owner, Path path) : base(owner, path)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    UpdateClutch();
    return true;
  }
}

public class POrderMoveByPath : POrderBase
{
  protected readonly Path _path;
  private Point? _lastPos;

  public POrderMoveByPath(Pac owner, GameField map, Point target) : base(owner)
  {
    _path = map.FindPath(Owner.Pos, target);
  }

  public POrderMoveByPath(Pac owner, Path path) : base(owner)
  {
    // Player.Print($"new {owner} to {path}");
    _path = path;
  }

  public List<Point> GetTurnPath()
  {
    var speed = Owner.IsBoosted ? 2 : 1;
    return _path.NextPath(Owner.Pos, speed);
  }

  public override bool IsCompleted(Context cx)
  {
    // Player.Print($"che {Owner} to {_path}");
    UpdateClutch();
    return Owner.IsInClutch || Owner.Pos == _path.Last();
  }

  public override bool Execute(Context cx)
  {
    _lastPos = Owner.Pos;

    var nextPoints = GetTurnPath();

    if (!TrafficLight.IsFree(cx, nextPoints))
    {
      _lastPos = null;
      return false;
    }

    Owner.Move(nextPoints.Last(), _path.Last().ToString());
    TrafficLight.Move(cx, nextPoints);
    return true;
  }

  protected void UpdateClutch()
  {
    Owner.IsInClutch = _lastPos.HasValue && _lastPos.Value == Owner.Pos;
  }
}