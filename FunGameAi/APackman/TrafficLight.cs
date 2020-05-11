using System.Collections.Generic;
using System.Linq;

public class TrafficLight
{
  public static readonly HashSet<Point> UsedPoints = new HashSet<Point>();
  public static void UpdateTick(Context cx)
  {
    UsedPoints.Clear();
  }

  public static bool IsFree(Context cx, Point target)
  {
    // todo pathfing
    var free = !UsedPoints.Contains(target);
    return free;
  }

  public static bool IsFree(Context cx, List<Point> nextPoints)
  {
    return !nextPoints.Any(UsedPoints.Contains);
  }

  public static void Move(Context cx, List<Point> points)
  {
    foreach (var point in points)
    {
      UsedPoints.Add(point);
    }
  }

  public static void Move(Context cx, Point target)
  {
    UsedPoints.Add(target);
  }
}