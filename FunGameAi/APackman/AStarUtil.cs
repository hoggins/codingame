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

      if (src.Pos == to)
        return ReconstructPath(cameFrom, src.Pos);

      for (int i = 0; i < 4; i++)
      {
        var adj = new Point(src.Pos.X + ColNum[i], src.Pos.Y + RowNum[i]);
        if (!IsValid(adj, rowLen, colLen)) continue;
        if (closedList[adj.ToIdx(rowLen)]) continue;
        if (!map.CanTraverse(adj)) continue;
        if (Warp(ref adj, rowLen, colLen) && !map.CanTraverse(adj)) continue;

        closedList[adj.ToIdx(rowLen)] = true;
        cameFrom[adj] = src.Pos;
        openList.Add(new Breadcrump(adj, adj.Distance(to)));
      }
    }

    return null;
  }

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
    var lastX = rowLen - 1;
    var lastY = colLen - 1;
    if (p.X == 0)
      p = new Point(lastX, p.Y);
    else if (p.X == lastX)
      p = new Point(0, (int) p.Y);
    else if (p.Y == 0)
      p = new Point(p.X, lastY);
    else if (p.Y == lastY)
      p = new Point((int) p.X, 0);
    else
      return false;
    return true;
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

  public static Point? FindNearest(this Map map, Point pos, CellFlags flags)
  {
    var openList = new List<Point>{pos};
    var nextOpenList = new List<Point>();

    var visited = new HashSet<int>();

    for (var i = 0;; i++)
    {
      if (i == openList.Count)
      {
        if (nextOpenList.Count == 0)
          return null;
        var sw = openList;
        openList = nextOpenList;
        nextOpenList = sw;
        nextOpenList.Clear();
        i = 0;
      }

      var c = openList[i];
      if (!IsValid(c, map.Grid.GetLength(1), map.Grid.GetLength(0))
          || (i != 0 && !map.CanTraverse(c)))
        continue;
      if (map.Grid[c.Y, c.X].Flags.HasFlag(flags))
        return c;
      FillNeighbors(c, visited, nextOpenList);
      FillPortal(map.Grid, c, visited, nextOpenList);
    }
  }

  private static void FillNeighbors(Point origin, HashSet<int> visited, List<Point> res)
  {
    void TryAdd(Point p)
    {
      if (visited.Add((p.X << 16) + (p.Y)))
        res.Add(p);
    }

    TryAdd(new Point(origin.X - 1, origin.Y));
    TryAdd(new Point(origin.X, origin.Y + 1));
    TryAdd(new Point(origin.X + 1, origin.Y));
    TryAdd(new Point(origin.X, origin.Y - 1));
  }

  private static void FillPortal(Cell[,] grid, Point point, HashSet<int> visited, List<Point> res)
  {
    void TryAdd(Point p)
    {
      if (visited.Add((p.X << 16) + (p.Y)))
        res.Add(p);
    }

    var lastX = grid.GetLength(1) - 1;
    if (point.X == 0)
      TryAdd(new Point(lastX, point.Y));
    if (point.X == lastX)
      TryAdd(new Point(0, (int) point.Y));

    var lastY = grid.GetLength(0) - 1;
    if (point.Y == 0)
      TryAdd(new Point(point.X, lastY));
    if (point.Y == lastY)
      TryAdd(new Point((int) point.X, 0));
  }

  #endregion


  private static bool IsValid(Point p, int rowLen, int colLen)
  {
    return p.X > 0 && p.X < rowLen && p.Y > 0 && p.Y < colLen;
  }
}