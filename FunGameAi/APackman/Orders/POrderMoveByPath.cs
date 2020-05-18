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
  public readonly Path _path;
  private Point? _lastPos;

  public POrderMoveByPath(Pac owner, GameField map, Point target) : base(owner)
  {
    _path = map.FindPath(Owner.Pos, target);
    _path.RemoveRange(0, _path.FindIndex(Owner.Pos)+1);
  }

  public POrderMoveByPath(Pac owner, Path path) : base(owner)
  {
    // Player.Print($"new {owner} to {path}");
    _path = path;
    _path.RemoveRange(0, _path.FindIndex(Owner.Pos)+1);
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
    return Owner.IsInClutch || _path.Count == 0;
  }

  public override bool Execute(Context cx)
  {
    _lastPos = Owner.Pos;

    var speed = Owner.IsBoosted ? 2 : 1;
    var nextPoints = _path.Take(speed).ToList();//GetTurnPath();

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
    if (_lastPos.HasValue && _lastPos.Value != Owner.Pos)
    {
      _path.RemoveAt(0);
      Owner.LastPath = _path;
    }
  }
}