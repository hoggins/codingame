using System;
using System.Collections.Generic;

public static class AStarUtil
{
  #region Find path

  private static bool[] ClosedList;

  private static int[] RowNum = {-1, 0, 0, 1};
  private static int[] ColNum = {0, -1, 1, 0};

  readonly struct Breadcrump
  {
    public readonly Point Pos;
    public readonly ushort Dist;

    public Breadcrump(Point pos, int dist)
    {
      Pos = pos;
      Dist = (ushort) dist;
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

    var cameFrom = new Dictionary<Point, Point>();
    var openList = new HashSet<Breadcrump> {new Breadcrump(@from, @from.Distance(to))};

    var rowLen = map.Grid.GetLength(1);
    var colLen = map.Grid.GetLength(0);
    closedList[from.ToIdx(rowLen)] = true;

    while (openList.Count > 0)
    {
      var src = openList.FindMin(n => n.Dist);
      openList.Remove(src);

      for (int i = 0; i < 4; i++)
      {
        var adj = new Point(src.Pos.X + ColNum[i], src.Pos.Y + RowNum[i]);
        Warp(ref adj, rowLen, colLen);
        if (!IsValid(adj, rowLen, colLen)) continue;
        if (closedList[adj.ToIdx(rowLen)]) continue;
        if (!map.CanTraverse(adj)) continue;

        closedList[adj.ToIdx(rowLen)] = true;
        cameFrom[adj] = src.Pos;

        if (adj == to)
          return ReconstructPath(cameFrom, src.Pos);
        openList.Add(new Breadcrump(adj, adj.Distance(to)));
      }
    }

    Player.Print($"path not found {from} {to}");
    return null;
  }

  private static Path ReconstructPath(Dictionary<Point, Point> cameFrom, Point src)
  {
    var path = new Path();
    path.Add(src);
    while (cameFrom.ContainsKey(src))
    {
      src = cameFrom[src];
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

  private static bool[] GetClosedList(Map map)
  {
    if (ClosedList == null)
      ClosedList = new bool[map.Grid.Length];
    else
      Array.Clear(ClosedList, 0, ClosedList.Length);
    var closedList = ClosedList;
    return closedList;
  }

  private static bool Warp(ref Point p, int rowLen, int colLen)
  {
    var lastX = rowLen;
    var lastY = colLen;
    if (p.X == -1)
      p = new Point(lastX, p.Y);
    else if (p.X == lastX)
      p = new Point(0, (int) p.Y);
    else if (p.Y == -1)
      p = new Point(p.X, lastY);
    else if (p.Y == lastY)
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