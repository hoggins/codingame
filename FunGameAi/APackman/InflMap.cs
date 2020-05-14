using System;
using System.Collections.Generic;
using System.Linq;

public class Map<T>
{
  public readonly T[] Grid;
  public readonly int Height;
  public readonly int Width;

  public Map(int height, int width)
  {
    Height = height;
    Width = width;
    Grid = new T[height * width];
  }

  public T this[Point p]
  {
    get { return Grid[p.ToIdx(Width)]; }
    set { Grid[p.ToIdx(Width)] = value; }
  }

  public void Clean()
  {
    Array.Clear(Grid, 0, Grid.Length);
  }

  public void Dump()
  {
    for (int i = 0; i < Height; i++)
    {
      for (int j = 0; j < Width; j++)
      {
        var idx = i * Width + j;
        Console.Error.Write(Grid[idx].ToString());
      }
      Console.Error.WriteLine();
    }
  }
}


public class InflMap
{
  private Point[][][] _cellConnections;

  public Map<float> CostMap;

  public void Init(Context cx)
  {
    var g = cx.Field.Grid;
    CostMap = new Map<float>(g.GetLength(0), g.GetLength(1));

    _cellConnections = new Point[g.Length][][];
    var rowLen = g.GetLength(1);
    for (int i = 0; i < g.GetLength(0); i++)
    {
      for (int j = 0; j < rowLen; j++)
      {
        var flags = g[i, j].Flags;
        if (flags.CHasFlag(CellFlags.Wall))
        {
          _cellConnections[i*rowLen + j] = new Point[0][];
          continue;
        }

        var points = FindNearest(cx.Field, new Point(j, i));
        _cellConnections[i * rowLen + j] = points.Select(r => r.ToArray()).ToArray();
      }
    }
  }

  public void TickUpdate(Context cx)
  {
    CostMap.Clean();

    var rowLen = cx.Field.Grid.GetLength(1);
    foreach (var pac in cx.Pacs)
    {
      if (!pac.IsMine)
        continue;
      var points = _cellConnections[pac.Pos.ToIdx(rowLen)];
      var cost = 2f;
      foreach (var bundle in points)
      {
        foreach (var point in bundle)
        {
          CostMap[point] += cost;
        }
        cost -= 0.1f;
      }
    }

    // Dump(rowLen);

  }

  private void Dump(int rowLen)
  {
    CostMap.Dump();

  }

  public static List<List<Point>> FindNearest(GameField gameField, Point pos, int hops = 10)
  {
    var openList = new List<Point>{pos};
    var nextOpenList = new List<Point>();

    var rowLen = gameField.Grid.GetLength(1);
    var colLen = gameField.Grid.GetLength(0);
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
}