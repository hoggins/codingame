using System.Collections.Generic;
using System.Linq;

public static class CryMapExtensions
{
  public static IEnumerable<T> EnumerateMap<T>(this T[,] map)
  {
    for (int j = 0; j < map.GetLength(1); j++)
    {
      for (int i = 0; i < map.GetLength(0); i++)
      {
        yield return map[i, j];
      }
    }
  }

  public static MapCell FindOreBest(this MapCell[,] map, Point fromPos, int minStack = 1)
  {
    var savedOre = map.EnumerateMap().Sum(c => c.IsDigged && c.IsSafe() ? c.Ore.GetValueOrDefault() : 0);
    var captureCoef = savedOre > 12 ? 1 : 7;

    var nearest = map.EnumerateMap()
      .Where(c=>(c.Ore >= minStack && c.IsSafe())
                || (c.IsSafe() && !c.Hole && !c.Ore.HasValue && c.InitialOre > 0))
      .FindMin(c=>CalcWeightedDist(fromPos, c, captureCoef));

    var leftMost = map.EnumerateMap()
      .Where(c => c.Ore >= minStack && c.IsSafe())
      .FirstOrDefault();

    if (leftMost != null && nearest.Pos.X - leftMost.Pos.X > 5)
      return leftMost;
    return nearest;
  }

  public static MapCell FindOreNearest(this MapCell[,] map, Point fromPos, int minStack = 1)
  {
    return map.EnumerateMap()
      .Where(c=>c.Ore >= minStack && c.IsSafe())
      .FindMin(c=>c.Distance(fromPos));
  }

  private static int CalcWeightedDist(Point fromPos, MapCell c, int coef = 7)
  {
    var crowdCoef = c.DigLock*2;
    var captureCoef = (c.IsDigged && c.Ore == 1 ? coef : 0);
    var preventMinignCoef = (c.IsDigged && c.Ore == 2 ? -4 : 0);
    return Utils.Distance(c.Pos, fromPos) + crowdCoef + captureCoef + preventMinignCoef;
  }

  public static MapCell FindNearestSafe(this MapCell[,] map, Point fromPos)
  {
    for (int i = 1; i < 29; i++)
    {
      foreach (var pos in Utils.EnumerateNeighbors(fromPos, i))
      {
        var cell = map[pos.Y, pos.X];
        if (cell.IsSafe())
          return cell;
      }
    }

    return null;
  }
}