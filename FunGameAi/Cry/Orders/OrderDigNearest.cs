public class OrderDigNearest : OrderDig
{
  private int _wasCloseAt = int.MinValue;
  public OrderDigNearest(Entity robot, Point pos) : base(robot, pos)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return cx.Tick > 1 && _wasCloseAt + 1 == cx.Tick;
  }

  public override string ProduceCommand(Context cx)
  {
    var cell = cx.Field.GetCell(Pos);

    if (!cell.IsSafe())
    {
      cell = cx.Field.Map.FindNearestSafe(Pos);
      if (cell == null)
      {
        Robot.Message = "no nearest";
        return null;
      }

      Pos = cell.Pos;
    }

    var distance = cell.Distance(Robot.Pos);
    if (distance <= 1)
      _wasCloseAt = cx.Tick;

    return base.ProduceCommand(cx);
  }
}