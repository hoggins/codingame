public class OrderRandomDig : EOrder
{
  private readonly (int, int) _target;

  private EOrder _subOrder;

  public OrderRandomDig(Entity robot) : base(robot)
  {
  }

  public static bool TryProduce(Context cx, Entity robot, out EOrder order)
  {
    order = null;
    if (robot.Item != ItemType.None)
      return false;
    order = new OrderRandomDig(robot);
    return true;
  }

  public override bool IsCompleted(Context cx)
  {
    if (Robot.Item != ItemType.None)
      return true;
    if (cx.VisibleOre > 3)
      return true;
    return false;
  }

  public override string ProduceCommand(Context cx)
  {
    if (_subOrder != null)
    {
      if (!_subOrder.IsCompleted(cx))
        return _subOrder.ProduceCommand(cx);
      _subOrder.Finalize(cx);
      _subOrder = null;
    }

    if (Robot.X == 0)
    {
      return Shift(cx, 4);
    }

    var command = DigFromCurrentLoc(cx);
    if (command != null)
      return command;
    if (Robot.X + 2 >= 30)
      return null;
    return Shift(cx, 2);
  }

  private string Shift(Context cx, int v)
  {
    _subOrder = new OrderDigOre(Robot, (Robot.X + v, Robot.Y));
    if (_subOrder.IsCompleted(cx))
      _subOrder = new OrderMove(Robot, (Robot.X + v, Robot.Y));
    return _subOrder.ProduceCommand(cx);
  }

  private string DigFromCurrentLoc(Context cx)
  {
    foreach (var pos in Utils.EnumerateNeighbors(Robot.Pos))
    {
      var dig = new OrderDigOre(Robot, pos);
      if (dig.IsCompleted(cx))
        continue;
      _subOrder = dig;
      return _subOrder.ProduceCommand(cx);
    }

    return null;
  }
}