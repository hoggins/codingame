using System.Linq;

public static class HighOrderHideScout
{
  public static bool TryGive(Context cx)
  {
    var point = HighOrderScout.PointToCover;
    if (cx.TrapCooldown > 0 || !point.HasValue)
      return false;

//      var r = cx.EnumerateRobots().FirstOrDefault(x=>!x.IsBusy(cx));
    var r = cx.EnumerateRobots()
      .Where(e => !e.IsBusy(cx) && e.Pos.Item1 == 0)
      .FindMin(e => Utils.Distance(e.Pos, point.Value));

    if (r == null)
      return false;

    HighOrderScout.PointToCover = null;

    var cell = cx.FindNearestSafe(point.Value);

    var newOrder = new OrderChain(r,
      new EOrder[]
      {
        //new OrderMove(r, (0, r.Y)),
        new OrderTake(r, ItemType.Trap),
        new OrderDigNearest(r, cell.Pos),
      }
    );
    r.Order = newOrder;
    return true;
  }
}