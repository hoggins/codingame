using System;
using System.Collections.Generic;
using System.Linq;

public static class AStarUtil
{
  #region Find path

  private static bool[] ClosedList;
  private static ushort[] WeightList;

  private static List<Breadcrump> OpenList;

  private static int[] RowNum = {-1, 0, 0, 1};
  private static int[] ColNum = {0, -1, 1, 0};

  readonly struct Breadcrump
  {
    public readonly Point Pos;
    public readonly short HScore;
    public readonly short GScore;
    public readonly byte Hops;
    public readonly CellFlags Flags;

    public Breadcrump(Point pos, int hops, int hScore, int gScore = 0, CellFlags flags = default)
    {
      Pos = pos;
      HScore = (short) hScore;
      GScore = (short) gScore;
      Hops = (byte) hops;
      Flags = flags;
    }

    public bool Equals(Breadcrump other)
    {
      return Pos.Equals(other.Pos);
    }

    public override bool Equals(object obj)
    {
      return obj is Breadcrump other && Equals(other);
    }

    public override int GetHashCode()
    {
      return Pos.GetHashCode();
    }
  }

  public static Path FindPath(this Map map, Point @from, Point to)
  {
    var closedList = GetClosedList(map);

    var cameFrom = new Dictionary<Point, Breadcrump>();
    var openList = new HashSet<Breadcrump> {new Breadcrump(@from, 0, @from.Distance(to))};

    var rowLen = map.Grid.GetLength(1);
    var colLen = map.Grid.GetLength(0);
    closedList[from.ToIdx(rowLen)] = true;

    while (openList.Count > 0)
    {
      var src = openList.FindMin(n => n.HScore);
      openList.Remove(src);

      for (int i = 0; i < 4; i++)
      {
        var adj = new Point(src.Pos.X + ColNum[i], src.Pos.Y + RowNum[i]);
        Warp(ref adj, rowLen, colLen);
        if (!IsValid(adj, rowLen, colLen)) continue;
        if (closedList[adj.ToIdx(rowLen)]) continue;
        if (!map.CanTraverse(adj)) continue;

        closedList[adj.ToIdx(rowLen)] = true;
        cameFrom[adj] = src;

        if (adj == to)
          return ReconstructPath(cameFrom, src.Pos);
        openList.Add(new Breadcrump(adj, src.Hops+1, adj.Distance(to)));
      }
    }

    Player.Print($"path not found {from} {to}");
    return null;
  }

  private static Path ReconstructPath(Dictionary<Point, Breadcrump> cameFrom, Point src, int value = 0)
  {
    var path = new Path{Value = value};
    path.Add(src);
    while (cameFrom.ContainsKey(src))
    {
      src = cameFrom[src].Pos;
      path.Add(src);
    }

    path.Reverse();
    return path;
  }

  #endregion

  #region Find Nearest

  public static Point? FindNearest(this Map map, Point pos, CellFlags flags, int minPath = 1)
  {
    var openList = new List<Point>{pos};
    var nextOpenList = new List<Point>();

    var rowLen = map.Grid.GetLength(1);
    var colLen = map.Grid.GetLength(0);
    var closedList = GetClosedList(map);

    var iterations = 1;
    for (var i = 0;; i++)
    {
      if (i == openList.Count)
      {
        if (nextOpenList.Count == 0)
          return null;
        ++iterations;
        var sw = openList;
        openList = nextOpenList;
        nextOpenList = sw;
        nextOpenList.Clear();
        i = 0;
      }

      var src = openList[i];

      for (int j = 0; j < 4; j++)
      {
        var adj = new Point(src.X + ColNum[j], src.Y + RowNum[j]);
        Warp(ref adj, rowLen, colLen);
        if (!IsValid(adj, rowLen, colLen)) continue;
        if (closedList[adj.ToIdx(rowLen)]) continue;
        if (!map.CanTraverse(adj)) continue;

        if (iterations >= minPath && map.Grid[adj.Y, adj.X].HasFlag(flags))
          return adj;

        closedList[adj.ToIdx(rowLen)] = true;
        nextOpenList.Add(adj);
      }
    }
  }

  #endregion

  #region Find beset Path

  public static Path FindBestPath(this Map map, Point from, int options, int lenght)
  {
    var weightList = GetWeightList(map);
    var rowLen = map.Grid.GetLength(1);
    var pathOptions = new List<Path>();
    var zeroPath = 0;
    for (var i = 0; i < options; i++)
    {
      var path = FindBestPath(map, from, weightList, lenght);
      if ((path == null || path.Value == 0) && ++zeroPath == 2)
        break;
      pathOptions.Add(path);
      foreach (var p in path)
      {
        ++weightList[p.ToIdx(rowLen)];
      }
    }
    var best = pathOptions.FindMax(p => p.Value / (double) p.Count);
    Player.Print($"options: \n" + string.Join("\n", pathOptions.Select(p=>PathStats(map, p))));
    // Player.Print("best " + best);
    return best;
  }

  private static string PathStats(Map map, Path path)
  {
    var flags = map.EnumeratePathFlags(path).ToList();
    var pellets = flags.Count(f => f.CHasFlag(CellFlags.HadPellet));
    var seen = flags.Count(f => !f.CHasFlag(CellFlags.Seen));
    var pacs = flags.Count(f => f.CHasFlag(CellFlags.EnemyPac) || f.CHasFlag(CellFlags.MyPac));
    var weight = path.Value / (double) path.Count;
    return $"w:{weight:0.00} v:{path.Value} pel:{pellets} see:{seen} pac:{pacs} : " +path;
  }

  public static Path FindBestPath(this Map map, Point @from, ushort[] cost, int length)
  {
    const int maxValue = 2;
    var closedList = GetClosedList(map);

    var cameFrom = new Dictionary<Point, Breadcrump>();
    var bStart = new Breadcrump(@from, 0, 0, 0);
    var openList = GetOpenList();
    openList.Add(bStart);

    var rowLen = map.Grid.GetLength(1);
    var colLen = map.Grid.GetLength(0);
    closedList[from.ToIdx(rowLen)] = true;

    var lastBest = (Breadcrump?) null;
    while (openList.Count > 0)
    {
      var src = openList.FindMax(n => n.HScore - n.GScore);
      lastBest = src;
      openList.Remove(src);

      if (src.Hops >= length)
        return ReconstructPath(cameFrom, src.Pos, src.HScore);

      var anyAdj = false;
      for (var i = 0; i < 4; i++)
      {
        var adj = new Point(src.Pos.X + ColNum[i], src.Pos.Y + RowNum[i]);
        Warp(ref adj, rowLen, colLen);
        if (!IsValid(adj, rowLen, colLen)) continue;

        var mapFlags = map.GetFlags(adj);
        if (mapFlags.CHasFlag(CellFlags.Wall))
          continue;

        if (closedList[adj.ToIdx(rowLen)]) continue;
        closedList[adj.ToIdx(rowLen)] = true;

        anyAdj = true;

        // if (flags.CHasFlag(CellFlags.EnemyPac) || flags.CHasFlag(CellFlags.MyPac))
          // continue;

        var adjFlags = CellFlags.Default;
        var adjValue = 0;
        if (mapFlags.CHasFlag(CellFlags.GemPellet) && !src.Flags.CHasFlag(CellFlags.MyPac) && !src.Flags.CHasFlag(CellFlags.EnemyPac))
          adjValue = 10;
        else if (mapFlags.CHasFlag(CellFlags.HadPellet))
          adjValue = maxValue;
        else if (!mapFlags.CHasFlag(CellFlags.Seen))
          adjValue = maxValue / 2;
        else if (mapFlags.CHasFlag(CellFlags.EnemyPac))
        {
          adjValue = -5;
          adjFlags |= CellFlags.EnemyPac;
        }
        else if (mapFlags.CHasFlag(CellFlags.MyPac))
        {
          adjValue = -2;
          adjFlags |= CellFlags.MyPac;
        }

        // inverse heuristic to make sum minimal
        var hScore = src.HScore + adjValue;
        var gScore = cost[adj.ToIdx(rowLen)];

        cameFrom[adj] = new Breadcrump(src.Pos, src.Hops+1, hScore, gScore, src.Flags | adjFlags);
        openList.Add(new Breadcrump(adj, src.Hops+1, hScore, gScore, src.Flags | adjFlags));
      }

      if (!anyAdj)
        return ReconstructPath(cameFrom, src.Pos, src.HScore);
    }

    // if (lastBest.HasValue)
      // return ReconstructPath(cameFrom, lastBest.Value.Pos, Math.Abs(lastBest.Value.HScore/maxValue));
    // todo add path to last node?
    Player.Print($"path not found {from} {length}");
    return null;
  }

  #endregion

  private static bool[] GetClosedList(Map map)
  {
    if (ClosedList == null)
      ClosedList = new bool[map.Grid.Length];
    else
      Array.Clear(ClosedList, 0, ClosedList.Length);
    var closedList = ClosedList;
    return closedList;
  }

  private static ushort[] GetWeightList(Map map)
  {
    if (WeightList == null)
      WeightList = new ushort[map.Grid.Length];
    else
      Array.Clear(WeightList, 0, WeightList.Length);
    var list = WeightList;
    return list;
  }

  private static List<Breadcrump> GetOpenList()
  {
    if (OpenList == null)
      OpenList = new List<Breadcrump>(64);
    else
      OpenList.Clear();
    return OpenList;
  }

  public static bool Warp(ref Point p, int rowLen, int colLen)
  {
    if (p.X == unchecked((ushort)-1))
      p = new Point(rowLen-1, p.Y);
    else if (p.X == rowLen)
      p = new Point(0, (int) p.Y);

    else if (p.Y == unchecked((ushort)-1))
      p = new Point(p.X, colLen-1);
    else if (p.Y == colLen)
      p = new Point((int) p.X, 0);
    else
      return false;
    return true;
  }


  private static bool IsValid(Point p, int rowLen, int colLen)
  {
    return p.X < rowLen && p.Y < colLen;
  }
}