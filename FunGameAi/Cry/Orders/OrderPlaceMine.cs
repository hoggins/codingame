public class OrderPlaceMine : EOrder
{
  private readonly Entity _robot;
  private Point? _scoutPoint;
  private bool _isLocked;
  private bool _isRequested;
  private bool _isReceived;
  private int _wasCloseAt;

  public OrderPlaceMine(Entity robot, Point scoutPoint) : base(robot)
  {
    _robot = robot;
    _scoutPoint = scoutPoint;
  }

  public override bool IsCompleted(Context cx)
  {
    return _isRequested && _isReceived && cx.Tick > 1 && _wasCloseAt + 1 == cx.Tick;;
  }

  public override string ProduceCommand(Context cx)
  {
    if (!_isLocked && _scoutPoint.HasValue)
    {
      _isLocked = true;
      cx.IncDigLock(_scoutPoint.Value);
    }

    if (!_isRequested)
    {
      if (_robot.X == 0)
      {
        _isRequested = true;
        return "REQUEST TRAP";
      }

      return Entity.Move((0, _robot.Y));
    }

    if (_robot.Item != ItemType.Trap && !_isReceived)
      return "REQUEST TRAP";


    if (_robot.Item == ItemType.Trap)
    {
      _isReceived = true;

      var forceSwitch = _scoutPoint.HasValue && !cx.Field.GetCell(_scoutPoint.Value).IsSafe();

      if (_scoutPoint.HasValue && Utils.Distance(_robot.Pos, _scoutPoint.Value) < 7)
        TrySetNewPoint(cx, cx.Field.Map.FindOreNearest(_scoutPoint.Value, 2), forceSwitch);

      if (!_scoutPoint.HasValue)
        TrySetNewPoint(cx, cx.Field.Map.FindOreNearest(_robot.Pos), forceSwitch);

      if (!_scoutPoint.HasValue)
        return null;

      if (Utils.Distance(_robot.Pos, _scoutPoint.Value) <= 1)
        _wasCloseAt = cx.Tick;

      var p = _scoutPoint.Value;
      return $"DIG {p.X} {p.Y}";
    }

    return null;
  }

  private void TrySetNewPoint(Context cx, MapCell vein, bool force)
  {
    if (force || vein == null)
    {
      if (_scoutPoint.HasValue)
        cx.DecDigLock(_scoutPoint.Value);
      _scoutPoint = null;
    }

    if (vein != null
        && (!_scoutPoint.HasValue || Utils.Distance(_robot.Pos, vein.Pos) < Utils.Distance(_robot.Pos, _scoutPoint.Value)))
    {
      if (_scoutPoint.HasValue)
        cx.DecDigLock(_scoutPoint.Value);
      _scoutPoint = vein.Pos;
      cx.IncDigLock(vein.Pos);
    }
  }
}