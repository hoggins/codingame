using System.Collections.Generic;
using System.Linq;

public static class HighOrderScout
{
  private const int OreToStart = 25;

  private static readonly (int, int)[] Points = new[]
  {
    (5, 4),
    (5, 12),
    (10, 7),
    (14, 3),
    (15, 11),
    (20, 8),
    (23, 2),
    (25, 10),
  };

  public static (int, int)? PointToCover { get; set; }
  private static ((int, int), int)? PointToPlace { get; set; }

  public static List<((int, int), int?)> MyRadars = new List<((int, int), int?)>();

  public static void TrySchedule(Context cx)
  {
    var isInProgress = cx.EnumerateRobots().Any(e => e.HasOrder<OrderDigNearestRadar>());
    if (isInProgress)
      return;

    var visibleOre = cx.VisibleOre;
    if (visibleOre > OreToStart)
      return;

    var safePoint = GetNextScoutPoint(cx);
    if (!safePoint.HasValue)
      return;

    var (scoutPoint, id) = safePoint.Value;
    if (!PickRobot(cx, scoutPoint, visibleOre, out var robot))
      return;

    if (id == 2 || id == 5 || id == 6)
      PointToCover = scoutPoint;
    PointToPlace = safePoint;

  }

  public static void TryGive(Context cx)
  {
    if (!PointToPlace.HasValue)
      return;
    var (scoutPoint, id) = PointToPlace.Value;

    var visibleOre = cx.VisibleOre;
    if (!PickRobot(cx, scoutPoint, visibleOre, out var robot))
      return;

    PointToPlace = null;
//      robot.Order = new OrderPlaceRadar(robot, scoutPoint, id);
    var newOrder = new OrderChain(robot,
      new EOrder[]
      {
        new OrderMove(robot, (0, robot.Y)),
        new OrderTake(robot, ItemType.Radar),
        new OrderDigNearestRadar(robot, scoutPoint, id),
      }
    );
    robot.Order = newOrder;
  }

  private static ((int, int), int)? GetNextScoutPoint(Context cx)
  {

//      for (var i = MyRadars.Count - 1; i >= 0; i--)
//      {
//        var (pos, id) = MyRadars[i];
//        var exist = cx.Entities.Any(e => e.X == pos.Item1 && e.Y == pos.Item2 && e.Type == EntityType.Radar);
//        if (!exist)
//        {
//          MyRadars.RemoveAt(i);
//        }
//      }

    for (var i = 0; i < Points.Length; i++)
    {
      var myRadar = MyRadars.Find(r => r.Item2.HasValue && r.Item2.Value == i);
      if (myRadar.Item2.HasValue)
        continue;
      var p = Points[i];
      var cell = cx.GetCell(p);
      if (!cell.IsSafe())
      {
        var c = cx.FindNearestSafe(p);
        return c == null ? default : (c.Pos, i);
      }

      return (p, i);
    }

    return null;
  }

  private static bool PickRobot(Context cx, (int, int) scoutPoint, int visibleOre, out Entity robot)
  {
    robot = cx.EnumerateRobots()
      .Where(e => !e.IsBusy(cx) && e.Pos.Item1 == 0)
      .FindMin(e => Utils.Distance(e.Pos, scoutPoint));

    if (robot == null)
    {
      // prevent hang when we have to switch to scout in field
      if (visibleOre == 0)
        robot = cx.EnumerateRobots()
          .Where(e => !e.IsBusy(cx, true))
          .FindMin(e => Utils.Distance(e.Pos, (0, e.Y)) + Utils.Distance(e.Pos, scoutPoint));

      if (robot == null)
        return false;
    }

    return true;
  }
}