using System;
using System.Collections.Generic;
using System.Linq;

public static class HighOrderScout
{
  private const int OreToStart = 25;

  private static readonly Point[] Points = new[]
  {
    new Point(5, 4),
    new Point(5, 12),
    new Point(10, 7),
    new Point(14, 3),
    new Point(15, 11),
    new Point(20, 8),
    new Point(23, 2),
    new Point(25, 10),
  };

  public static Point? PointToCover { get; set; }
  private static (Point Pos, int i)? PointToPlace { get; set; }

  public static List<(Point, int?)> MyRadars = new List<(Point, int?)>();

  public static void TrySchedule(Context cx)
  {
    var visibleOre = cx.VisibleOre;
    if (visibleOre > OreToStart)
      return;

    var scheduledForOther = cx.EnumerateRobots().Where(x => x.HasOrder<OrderDigNearestRadar>()).ToList();

    var radarIdBusy = scheduledForOther
      .Any(x=> (x.Order is OrderChain chain) && chain.FirstOrder is OrderMove);
    if (radarIdBusy)
      return;

    var excludePoiunts = scheduledForOther.Select(x => x.GetOrder<OrderDigNearestRadar>().RadarId).ToList();
    var safePoint = GetNextScoutPoint(cx, excludePoiunts);
    if (!safePoint.HasValue)
      return;

    var (scoutPoint, id) = safePoint.Value;
    if (!PickRobot(cx, scoutPoint, visibleOre, out var robot))
      return;

    var eta = (int)Math.Ceiling(robot.X / 4d);
    if (cx.RadarCooldown > eta)
      return;

    // if (id == 2 || id == 5 || id == 6)
      // PointToCover = scoutPoint;
    // PointToPlace = safePoint;
    GiveOrder(robot, scoutPoint, id);
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
    GiveOrder(robot, scoutPoint, id);
  }

  private static void GiveOrder(Entity robot, Point scoutPoint, int id)
  {
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

  private static (Point Pos, int i)? GetNextScoutPoint(Context cx, List<int> excludePoints)
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
      var myRadar = MyRadars.Any(r => r.Item2 == i);
      if (myRadar)
        continue;
      if (excludePoints.Contains(i))
        continue;

      var p = Points[i];
      var cell = cx.Field.GetCell(p);
      if (!cell.IsSafe())
      {
        var c = cx.Field.Map.FindNearestSafe(p);
        return c == null ? default : (c.Pos, i);
      }

      return (p, i);
    }

    return null;
  }

  private static bool PickRobot(Context cx, Point scoutPoint, int visibleOre, out Entity robot)
  {
    robot = cx.EnumerateRobots()
      .Where(e => !e.IsBusy(cx) && e.Pos.X == 0)
      .FindMin(e => Utils.Distance(e.Pos, scoutPoint));

    if (robot == null)
    {
      // prevent hang when we have to switch to scout in field
      if (visibleOre == 0)
        robot = cx.EnumerateRobots()
          .Where(e => !e.IsBusy(cx, true))
          .FindMin(e => Utils.Distance(e.Pos, new Point(0, e.Y)) + Utils.Distance(e.Pos, scoutPoint));

      if (robot == null)
        return false;
    }

    return true;
  }
}