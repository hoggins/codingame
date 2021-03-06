using System;
using System.Collections.Generic;
using System.Linq;

public static class Balance
{
  public static float GetCellValue(CellFlags mapFlags)
  {
    var adjValue = -0.2f;

    if (mapFlags.CHasFlag(CellFlags.Wall))
      adjValue = 0;
    else if (mapFlags.CHasFlag(CellFlags.GemPellet))
      adjValue = 10;
    else if (mapFlags.CHasFlag(CellFlags.HadPellet))
      adjValue = 2;
    else if (!mapFlags.CHasFlag(CellFlags.Seen))
      adjValue = 1;
    else if (mapFlags.CHasFlag(CellFlags.EnemyPac))
      adjValue = -5;
    else if (mapFlags.CHasFlag(CellFlags.MyPac))
      adjValue = -2;
    else
      return -0.2f;

    return adjValue;
  }
}

public static class AStarUtil
{
  #region Find path

  private static bool[] ClosedList;
  private static bool[] OpenProcessedList;
  private static ushort[] WeightList;

  private static List<Breadcrump> OpenList;

  public static int[] RowNum = {-1, 0, 0, 1};
  public static int[] ColNum = {0, -1, 1, 0};

  class Breadcrump
  {
    public Point Pos;
    public float HScore;
    public float GScore;
    public byte Hops;
    public CellFlags Flags;

    public Breadcrump(Point pos, int hops, float hScore, float gScore = 0, CellFlags flags = default)
    {
      Pos = pos;
      HScore = hScore;
      GScore = gScore;
      Hops = (byte) hops;
      Flags = flags;
    }

    public Breadcrump Set(Point pos, int hops, float hScore, float gScore = 0, CellFlags flags = default)
    {
      Pos = pos;
      HScore = hScore;
      GScore = gScore;
      Hops = (byte) hops;
      Flags = flags;
      return this;
    }

    public Breadcrump()
    {

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

  private static Pool<Breadcrump> _breadcrumps = new Pool<Breadcrump>();

  public static Path FindPath(this GameField gameField, Point @from, Point to)
  {
    var closedList = GetClosedList(gameField);

    var cameFrom = new Dictionary<Point, Breadcrump>();
    var openList = new HashSet<Breadcrump> {new Breadcrump(@from, 0, @from.Distance(to))};

    var rowLen = gameField.Width;
    var colLen = gameField.Height;
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
        if (!gameField.CanTraverse(adj)) continue;

        closedList[adj.ToIdx(rowLen)] = true;
        cameFrom[adj] = src;

        if (adj == to)
          return ReconstructPath(cameFrom, to);
        openList.Add(new Breadcrump(adj, src.Hops+1, adj.Distance(to)));
      }
    }

    Player.Print($"path not found {from} {to}");
    return null;
  }

  private static Path ReconstructPath(Dictionary<Point, Breadcrump> cameFrom, Point src, float value = 0)
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

  public static Point? FindNearest(this GameField gameField, Point pos, CellFlags flags, int minPath = 1)
  {
    var openList = new List<Point>{pos};
    var nextOpenList = new List<Point>();

    var rowLen = gameField.Width;
    var colLen = gameField.Height;
    var closedList = GetClosedList(gameField);

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
        if (!gameField.CanTraverse(adj)) continue;

        if (iterations >= minPath && gameField.GetFlags(adj).CHasFlag(flags))
          return adj;

        closedList[adj.ToIdx(rowLen)] = true;
        nextOpenList.Add(adj);
      }
    }
  }

  #endregion

  #region Find beset Path

  public static List<Path> FindMultyPath(this GameField gameField, Point from, int options, int lenght,
    Map<float> cost = null)
  {
    var weightList = GetWeightList(gameField);

    var rowLen = gameField.Width;
    var pathOptions = new List<Path>();
    var zeroPath = 0;
    for (var i = 0; i < options; i++)
    {
      var path = FindBestPath(gameField, from, weightList, cost, lenght);
      if ((path == null) && ++zeroPath == 2)
        break;
      if (path != null)
      {
        pathOptions.Add(path);
        foreach (var p in path)
        {
          ++weightList[p.ToIdx(rowLen)];
        }
      }
    }
    return pathOptions;
  }

  public static Path FindBestPath(this GameField gameField, Point from, int options, int lenght, Map<float> cost = null, int realStart = 0, int realLen = 0)
  {
    var weightList = GetWeightList(gameField);

    var rowLen = gameField.Width;
    var pathOptions = new List<Path>();
    var zeroPath = 0;
    for (var i = 0; i < options; i++)
    {
      var path = FindBestPath(gameField, from, weightList, cost, lenght, realStart, realLen);
      if ((path == null) && ++zeroPath == 2)
        break;
      pathOptions.Add(path);
      foreach (var p in path)
      {
        ++weightList[p.ToIdx(rowLen)];
      }
    }
    var best = pathOptions.FindMax(p => p.Value / (p.Count-1));
    // Player.Print($"options: \n" + string.Join("\n", pathOptions.Select(p=>PathStats(gameField, cost, p))));
    // Player.Print("best " + best);
    return best;
  }

  private static Func<Breadcrump, float> ScoreClosure(GameField f, bool[] openList)
  {
    var w = f.Width;
    return (n) =>
    {
      var p = n.Pos;
      return openList[p.Y * w + p.X] ? float.MinValue : 0 - n.GScore;
    };
  }

  public static Path FindBestPath(this GameField gameField, Point @from, ushort[] cost, Map<float> cost2, int length, int realStart = 0, int realLen = 0)
  {
    var closedList = GetClosedList(gameField);
    var openProcessed = GetOpenProcessedList(gameField);

    var cameFrom = new Dictionary<Point, Breadcrump>();
    var bStart = new Breadcrump(@from, 0, 0, 0);
    var openList = GetOpenList();
    openList.Add(bStart);

    var rowLen = gameField.Width;
    var colLen = gameField.Height;
    closedList[from.ToIdx(rowLen)] = true;

    var filter = ScoreClosure(gameField, openProcessed);

    using var disCx = new DisposeContext();
    disCx.Disposed += () =>
    {
      foreach (var bc in cameFrom)
        _breadcrumps.Put(bc.Value);
    };
    disCx.Disposed += () =>
    {
      foreach (var bc in openList)
        _breadcrumps.Put(bc);
    };

    while (openList.Count > 0)
    {
      var src = openList.FindMax(filter);
      var srcPos = src.Pos;
      var srcIdx = srcPos.Y*rowLen+srcPos.X;
      if (openProcessed[srcIdx])
        break;
      openProcessed[srcIdx] = true;

      // openList.Remove(src);

      if (src.Hops >= length)
        return ReconstructPath(cameFrom, srcPos, src.HScore);

      var anyAdj = false;
      for (var i = 0; i < 4; i++)
      {
        var adj = new Point(srcPos.X + ColNum[i], srcPos.Y + RowNum[i]);
        Warp(ref adj, rowLen, colLen);
        if (!IsValid(adj, rowLen, colLen)) continue;

        var mapFlags = gameField.GetFlags(adj);
        if (mapFlags.CHasFlag(CellFlags.Wall))
          continue;

        var adjIdx = adj.Y*rowLen+adj.X;
        if (closedList[adjIdx]) continue;
        closedList[adjIdx] = true;

        anyAdj = true;

        var adjValue = Balance.GetCellValue(mapFlags);

        /*if (mapFlags.CHasFlag(CellFlags.GemPellet)
            && src.Hops < 3
            && !src.Flags.CHasFlag(CellFlags.MyPac) && !src.Flags.CHasFlag(CellFlags.EnemyPac))
          adjValue = 0f;*/

        // inverse heuristic to make sum minimal
        var cellScore = realLen > 0
          ? adjValue * (1 - (realStart + src.Hops + 1) / (float)realLen)
          : adjValue;
        var hScore = src.HScore + cellScore;
        var gScore = cost[adjIdx] + (cost2?[adj] ?? 0);


        cameFrom[adj] = _breadcrumps.Take().Set(srcPos, src.Hops+1, hScore, gScore, src.Flags | mapFlags);
        openList.Add(_breadcrumps.Take().Set(adj, src.Hops+1, hScore, gScore, src.Flags | mapFlags));
      }

      if (!anyAdj)
        return ReconstructPath(cameFrom, srcPos, src.HScore);
    }

    // if (lastBest.HasValue)
      // return ReconstructPath(cameFrom, lastBest.Value.Pos, Math.Abs(lastBest.Value.HScore/maxValue));
    // todo add path to last node?
    Player.Print($"path not found {from} {length}");
    return null;
  }

  class DisposeContext : IDisposable
  {
    public event Action Disposed;

    public DisposeContext()
    {

    }

    public void Dispose()
    {
      Disposed?.Invoke();
    }
  }

  class Pool<T> where T : new()
  {
    public List<T> _buffer = new List<T>();

    public T Take()
    {
      if (_buffer.Count == 0)
        return new T();
      var res = _buffer[_buffer.Count-1];
      _buffer.RemoveAt(_buffer.Count-1);
      return res;
    }

    public void Put(T obj)
    {
      _buffer.Add(obj);
    }
  }

  private static string PathStats(GameField gameField, Map<float> costMap, Path path)
  {
    var flags = Enumerable.ToList(gameField.EnumeratePathFlags(path));
    var pellets = flags.Count(f => f.CHasFlag(CellFlags.HadPellet));
    var seen = flags.Count(f => !f.CHasFlag(CellFlags.Seen));
    var pacs = flags.Count(f => f.CHasFlag(CellFlags.EnemyPac) || f.CHasFlag(CellFlags.MyPac));
    var weight = path.Value / path.Count;
    var cost = path.Sum(p => costMap?[p] ?? 0);
    return $"w:{weight:0.00} v:{path.Value:0.0} c:{cost:0.0} pel:{pellets} see:{seen} pac:{pacs} : " +path;
  }

  #endregion

  #region for inf
  public static List<List<Point>> FindNearest(GameField gameField, Point pos, int hops = 10)
  {
    var openList = new List<Point>{pos};
    var nextOpenList = new List<Point>();

    var rowLen = gameField.Width;
    var colLen = gameField.Height;
    var closedList = AStarUtil.GetClosedList(gameField);

    var res = new List<List<Point>>(hops);
    var hopList = new List<Point>(8);
    res.Add(hopList);
    var iterations = 1;
    for (var i = 0;; i++)
    {
      if (i == openList.Count)
      {
        ++iterations;
        if (nextOpenList.Count == 0 || iterations == hops)
          return res;

        hopList = new List<Point>();
        res.Add(hopList);

        var sw = openList;
        openList = nextOpenList;
        nextOpenList = sw;
        nextOpenList.Clear();
        i = 0;
      }

      var src = openList[i];

      for (int j = 0; j < 4; j++)
      {
        var adj = new Point(src.X + AStarUtil.ColNum[j], src.Y + AStarUtil.RowNum[j]);
        AStarUtil.Warp(ref adj, rowLen, colLen);
        if (!AStarUtil.IsValid(adj, rowLen, colLen)) continue;
        if (closedList[adj.ToIdx(rowLen)]) continue;
        if (!gameField.CanTraverse(adj)) continue;

        closedList[adj.ToIdx(rowLen)] = true;
        nextOpenList.Add(adj);

        hopList.Add(adj);
      }
    }
  }
  #endregion

  public static bool[] GetClosedList(GameField gameField)
  {
    if (ClosedList == null)
      ClosedList = new bool[gameField.Length];
    else
      Array.Clear(ClosedList, 0, ClosedList.Length);
    var closedList = ClosedList;
    return closedList;
  }

  public static ushort[] GetWeightList(GameField gameField)
  {
    if (WeightList == null)
      WeightList = new ushort[gameField.Length];
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

  public static bool[] GetOpenProcessedList(GameField gameField)
  {
    if (OpenProcessedList == null)
      OpenProcessedList = new bool[gameField.Length];
    else
      Array.Clear(OpenProcessedList, 0, OpenProcessedList.Length);
    var closedList = OpenProcessedList;
    return closedList;
  }

  public static bool Warp(ref Point p, int width, int colLen)
  {
    if (p.X == unchecked((ushort)-1))
      p = new Point(width-1, p.Y);
    else if (p.X == width)
      p = new Point(0, (int) p.Y);

    else if (p.Y == unchecked((ushort)-1))
      p = new Point(p.X, colLen-1);
    else if (p.Y == colLen)
      p = new Point((int) p.X, 0);
    else
      return false;
    return true;
  }


  public static bool IsValid(Point p, int rowLen, int colLen)
  {
    return p.X < rowLen && p.Y < colLen;
  }
}