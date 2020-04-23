using System.Linq;

public static class HighOrderMine
{
  public static void TryGive(Context cx)
  {
    if (cx.TrapCooldown > 0)
      return;

    if (cx.VisibleOre < 12)
      return;

//      if (cx.MyScore < cx.OpponentScore)
//        return;

    var robot = cx.EnumerateRobots()
      .FirstOrDefault(e => !e.IsBusy(cx) && e.Pos.Item1 == 0);

    if (robot == null)
      return;

    var vein = cx.FindOreNearest(robot.Pos, 2);
    if (vein == null)
      return;

    robot.Order = new OrderPlaceMine(robot, vein.Pos);
  }
}