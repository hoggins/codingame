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
    var point = nextPoints.First();
    return !cx.Map.GetFlags(point).CHasFlag(CellFlags.MyPac) && !UsedPoints.Contains(point); //!nextPoints.Any(UsedPoints.Contains);
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