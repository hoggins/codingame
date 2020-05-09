using System;
using System.Collections.Generic;
using System.Linq;

/**
 * Grab the pellets as fast as you can!
 **/
public static class Player
{
  public static void Print(string s) => Console.Error.WriteLine(s);

  private static void Main(string[] args)
  {
    var cx = new Context();
    cx.ReadInit();

    cx.Map.Dump();

    // game loop
    while (true)
    {
      cx.ReadTick();

      // var near = cx.Map.FindNearest(cx.Pacs.First().Pos, CellFlags.Pellet);
      // var had = cx.Map.FindNearest(cx.Pacs.First().Pos, CellFlags.HadPellet);
      // var seen = cx.Map.FindNearest(cx.Pacs.First().Pos, ~CellFlags.Seen);

      if (cx.Tick == 1)
      {
        var pellets = cx.Map.FindPellet(10).ToList();
        var weights = cx.Pacs.Where(p => p.IsMine)
          .SelectMany(p=>pellets.Select(l=>(pellets:l, pac:p, path:cx.Map.FindPath(p.Pos, l.Pos))))
          .Where(p=>p.path != null)
          .OrderBy(p=>p.path.Count)
          .ToList();

        var allocatedPellets = new List<Point>();
        foreach (var w in weights)
        {
          if (allocatedPellets.Contains(w.pellets.Pos)
              || w.pac.Order != null)
            continue;
          allocatedPellets.Add(w.pellets.Pos);
          w.pac.Order = new POrderMoveTo(w.pac, w.pellets.Pos);
        }
      }


      foreach (var pac in cx.Pacs.Where(p=>p.IsMine))
      {
        BehTree.Test(cx, pac);

        pac.Order?.Execute(cx);
      }

      Console.WriteLine();
    }
  }
}