using System.Collections.Generic;
using System.Linq;

public class Context
{
  public int Tick;
  public MapCell[,] Map;
  public List<Entity> Entities = new List<Entity>();
  public int RadarCooldown;
  public int TrapCooldown;
  public int MyScore;
  public int OpponentScore;

  public int VisibleOre => (int) (_visibleOre ?? (_visibleOre = CalcVisibleOre()));

  private int? _visibleOre;

  public IEnumerable<Entity> EnumerateRobots(bool includeDead = false) =>
    Entities.Where(e => e.Type == EntityType.Robot && (includeDead && e.IsDead || !e.IsDead));

  public IEnumerable<MapCell> EnumerateMap()
  {
    for (int j = 0; j < Map.GetLength(1); j++)
    {
      for (int i = 0; i < Map.GetLength(0); i++)
      {
        yield return Map[i, j];
      }
    }
  }

  public MapCell GetCell((int, int) pos)
  {
    return Map[pos.Item1, pos.Item2];
  }

  public MapCell FindOreBest((int, int) fromPos, int minStack = 1)
  {
    var savedOre = EnumerateMap().Sum(c => c.IsDigged && c.IsSafe() ? c.Ore.GetValueOrDefault() : 0);
    var captureCoef = savedOre > 12 ? 1 : 7;

    var nearest = EnumerateMap()
      .Where(c=>c.Ore >= minStack && c.IsSafe())
      .FindMin(c=>CalcWeightedDist(fromPos, c, captureCoef));

    var leftMost = EnumerateMap()
      .Where(c => c.Ore >= minStack && c.IsSafe())
      .FirstOrDefault();

    if (leftMost != null && nearest.Pos.Item1 - leftMost.Pos.Item1 > 5)
      return leftMost;
    return nearest;
  }

  public MapCell FindOreNearest((int, int) fromPos, int minStack = 1)
  {
    return EnumerateMap()
      .Where(c=>c.Ore >= minStack && c.IsSafe())
      .FindMin(c=>c.Distance(fromPos));
  }

  private static int CalcWeightedDist((int, int) fromPos, MapCell c, int coef = 7)
  {
    var crowdCoef = c.DigLock*2;
    var captureCoef = (c.IsDigged && c.Ore == 1 ? coef : 0);
    var preventMinignCoef = (c.IsDigged && c.Ore == 2 ? -4 : 0);
    return Utils.Distance(c.Pos, fromPos) + crowdCoef + captureCoef + preventMinignCoef;
  }

  public (int, int)? FindMineCell((int, int) fromPos)
  {
    var entity = Entities.Where(e=>e.Type == EntityType.Trap).FindMin(c=>Utils.Distance(c.Pos, fromPos));
    return entity?.Pos;
  }

  public MapCell FindNearestSafe((int, int) fromPos)
  {
    for (int i = 1; i < 29; i++)
    {
      foreach (var pos in Utils.EnumerateNeighbors(fromPos, i))
      {
        var cell = GetCell(pos);
        if (cell.IsSafe())
          return cell;
      }
    }

    return null;
  }

  public void PatchMap()
  {
    foreach (var e in Entities)
    {
      if (e.Type == EntityType.Trap)
        Map[e.X, e.Y].IsMined = true;
    }
  }

  public void IncDigLock((int, int) target)
  {
    var cell = GetCell(target);
    cell.DigLock += 1;
  }

  public void DecDigLock((int, int) target)
  {
    var cell = GetCell(target);
    cell.DigLock -= 1;
  }

  public void SetDigged((int, int) target)
  {
    Map[target.Item1, target.Item2].IncreaseDig();
  }

  public void ResetTick()
  {
    _visibleOre = null;
  }

  private int CalcVisibleOre()
  {
    return EnumerateMap().Sum(c => !c.IsSafe() ? 0 : c.Ore.GetValueOrDefault());
  }
}