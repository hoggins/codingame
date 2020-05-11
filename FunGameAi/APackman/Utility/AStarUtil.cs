using System;
using System.Collections.Generic;
using System.Linq;

public static class AStarUtil
{
  #region Find path

  private static bool[] ClosedList;
  private static ushort[] WeightList;

  private static int[] RowNum = {-1, 0, 0, 1};
  private static int[] ColNum = {0, -1, 1, 0};

  readonly struct Breadcrump
  {
    public readonly Point Pos;
    public readonly short HScore;
    public readonly short GScore;
    public readonly byte Hops;

    public Breadcrump(Point pos, int hops, int hScore, int gScore = 0)
    {
      Pos = pos;
      HScore = (short) hScore;
      GScore = (short) gScore;
      Hops = (byte) hops;
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
        openList.Add(new Breadcrump(adj, 0, adj.Distance(to)));
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
    for (var i = 0; i < options; i++)
    {
      var path = FindBestPath(map, from, weightList, lenght);
      if (path == null || path.Value == 0)
      {
        if (pathOptions.Count > 0)
          break;
        var nPoint = FindNearest(map, from, ~CellFlags.Seen) ?? FindNearest(map, from, CellFlags.HadPellet);
        if (nPoint.HasValue)
          return FindPath(map, from, nPoint.Value);
        return null;
      }
      pathOptions.Add(path);
      foreach (var p in path)
      {
        ++weightList[p.ToIdx(rowLen)];
      }
    }
    var best = pathOptions.FindMax(p => p.Value / (double) p.Count);
    // Player.Print($"options: \n->" + string.Join("\n->", pathOptions.Select(p=>$"{p.Value / (double) p.Count:0.00} : {p}")));
    // Player.Print("best " + best);
    return best;
  }

  public static Path FindBestPath(this Map map, Point @from, ushort[] weight, int length)
  {
    const int maxValue = 2;
    var closedList = GetClosedList(map);

    var cameFrom = new Dictionary<Point, Breadcrump>();
    var openList = new HashSet<Breadcrump> {new Breadcrump(@from, 0, 0, 0)};

    var rowLen = map.Grid.GetLength(1);
    var colLen = map.Grid.GetLength(0);
    closedList[from.ToIdx(rowLen)] = true;

    var lastBest = (Breadcrump?) null;
    while (openList.Count > 0)
    {
      var src = openList.FindMin(n => n.HScore + n.GScore);
      lastBest = src;
      openList.Remove(src);

      if (src.Hops >= length)
        return ReconstructPath(cameFrom, src.Pos, Math.Abs(src.HScore/maxValue));

      var anyAdj = false;
      for (var i = 0; i < 4; i++)
      {
        var adj = new Point(src.Pos.X + ColNum[i], src.Pos.Y + RowNum[i]);
        Warp(ref adj, rowLen, colLen);
        if (!IsValid(adj, rowLen, colLen)) continue;

        var flags = map.GetFlags(adj);
        if (flags.CHasFlag(CellFlags.Wall))
          continue;

        if (closedList[adj.ToIdx(rowLen)]) continue;
        closedList[adj.ToIdx(rowLen)] = true;

        anyAdj = true;

        // if (flags.CHasFlag(CellFlags.EnemyPac) || flags.CHasFlag(CellFlags.MyPac))
          // continue;

        var adjValue = 0;
        if (flags.CHasFlag(CellFlags.HadPellet))
          adjValue = maxValue;
        else if (!flags.CHasFlag(CellFlags.Seen))
          adjValue = maxValue / 2;
        else if (flags.CHasFlag(CellFlags.EnemyPac))
          adjValue = -maxValue;
        else if (flags.CHasFlag(CellFlags.MyPac))
          adjValue = maxValue / -1;

        // inverse heuristic to make sum minimal
        var hScore = src.HScore - adjValue;
        var gScore = adjValue != 0
          ? Math.Max(0, weight[adj.ToIdx(rowLen)] + hScore)
          : weight[adj.ToIdx(rowLen)];

        cameFrom[adj] = new Breadcrump(src.Pos, src.Hops+1, hScore, gScore);
        openList.Add(new Breadcrump(adj, src.Hops+1, hScore, gScore));
      }

      if (!anyAdj)
        return ReconstructPath(cameFrom, src.Pos, Math.Abs(src.HScore/maxValue));
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