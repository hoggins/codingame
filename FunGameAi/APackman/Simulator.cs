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
  private Map<ushort> _baseCost;
  private GameField _field;
  private SimInflMap _inflMap;

  public Simulator(int h, int w)
  {
    _field = new GameField();
    _baseCost = new Map<ushort>(h,w);
    _inflMap = new SimInflMap(_field,_baseCost, h, w);
  }

  public Dictionary<Pac, Path> RunBest(GameField srcField, List<Pac> srcPacs)
  {
    var gens = Run(srcField, srcPacs);

    var best = gens.FindMax(g => g.Sum(p => p.Geathered));
    return best.ToDictionary(
      b => srcPacs.First(p => p.Id == b.Id),
      p => p.LastPath);
  }

  public List<List<SimPac>> Run(GameField srcField, List<Pac> srcPacs)
  {
    _baseCost.Clean();

    var generations = new List<List<SimPac>>();

    for (int i = 0; i < 7; i++)
    {
      _field.CopyFrom(srcField);
      _inflMap.Clean();

      var pacs = srcPacs.Where(p => p.IsMine).Select(p => new SimPac(p)).ToList();
      //Reorder(pacs, i);
      SimulatePacs(_field, _inflMap, pacs, 20);

      generations.Add(pacs);
      foreach (var p in pacs.SelectMany(p=>p.LastPath))
      {
        ++_baseCost[p];
      }
    }

    return generations;
  }

  private void Reorder(List<SimPac> pacs, int i)
  {
    pacs[0] = pacs[3];
    pacs.RemoveRange(1, pacs.Count-1);
    return;
    var r = pacs[0];
    pacs[0] = pacs[i % pacs.Count];
    pacs[i % pacs.Count] = r;
  }

  private void SimulatePacs(GameField field, SimInflMap inflMap, List<SimPac> pacs, int hops)
  {

    var time = 0;
    do
    {
      inflMap.Update();
      inflMap.PlacePacs(pacs);

      var timeLeft = hops - time;
      foreach (var pac in pacs)
      {
        if (pac.CurPath.Count > 0)
          continue;
        var len = Math.Min(10, timeLeft);
        var bestPath = field.FindBestPath(pac.Pos, 8, len, inflMap.CostMap, pac.LastPath.Count, hops);
        pac.CurPath = bestPath;

        foreach (var p in bestPath)
        {
          ++inflMap.CostMap[p];
        }
      }

      var step = Math.Min(timeLeft, pacs.Min(p => p.CurPath.Count));
      foreach (var pac in pacs)
      {
        pac.LastPath.AddRange(pac.CurPath.Take(step));
        pac.Geathered += VisitPath(field, pac.CurPath.Take(step));
        pac.LastPath.Value = pac.Geathered;

        pac.CurPath.RemoveRange(0, step);

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