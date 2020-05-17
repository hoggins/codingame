using System;
using System.Collections.Generic;
using System.Linq;

public class SimPac
{
  public int Id;
  public Point Pos;
  public Path CurPath = new Path();
  public Path LastPath = new Path();

  public int Geathered;

  public SimPac(Pac pac)
  {
    Id = pac.Id;
    Pos = pac.Pos;
  }
}

public class SimContext
{
  public GameField Field;
  public List<SimPac> Pacs;
}

public class Simulator
{
  public List<List<SimPac>> Run(GameField srcField, List<Pac> srcPacs)
  {
    var baseCost = new Map<ushort>(srcField.Height, srcField.Width);

    var field = srcField.Clone();
    var pacs = srcPacs.Where(p => p.IsMine).Select(p => new SimPac(p)).ToList();
    var inflMap = new SimInflMap(field,baseCost);
    SimulatePacs(field, inflMap, pacs, 20);

    var generations = new List<List<SimPac>>();
    generations.Add(pacs);

    return generations;
  }

  private void SimulatePacs(GameField field, SimInflMap inflMap, List<SimPac> pacs, int hops)
  {

    var time = 0;
    do
    {
      inflMap.PlacePacs(pacs);

      foreach (var pac in pacs)
      {
        if (pac.CurPath.Count > 0)
          continue;
        var bestPath = field.FindBestPath(pac.Pos, 8, 10, inflMap.CostMap);
        pac.CurPath = bestPath;
      }

      var step = Math.Min(hops - time, pacs.Min(p => p.CurPath.Count));
      foreach (var pac in pacs)
      {
        var traversed = pac.CurPath.Take(step);
        pac.LastPath.AddRange(traversed);
        pac.CurPath.RemoveRange(0, step);

        pac.Geathered = VisitPath(field, traversed);

        pac.Pos = pac.LastPath.Last();
      }

      time += step;
    } while (time < hops);

  }

  private int VisitPath(GameField field, IEnumerable<Point> traversed)
  {
    var pellets = 0;
    foreach (var point in traversed)
    {
      pellets += field.Visit(point);
    }

    return pellets;
  }
}