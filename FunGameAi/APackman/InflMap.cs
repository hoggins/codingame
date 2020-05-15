using System.Collections.Generic;
using System.Linq;


public class InflMap
{
  private Map<Point[][]> _cellConnections;

  public Map<float> CostMap;

  public void Init(Context cx)
  {
    var g = cx.Field;
    CostMap = new Map<float>(g.Height, g.Width);

    _cellConnections = new Map<Point[][]>(g.Height, g.Width);

    for (int i = 0; i < g.Height; i++)
    {
      for (int j = 0; j < g.Width; j++)
      {
        var flags = g.GetFlags(i,j);
        if (flags.CHasFlag(CellFlags.Wall))
        {
          _cellConnections[i, j] = new Point[0][];
          continue;
        }

        var points = FindNearest(cx.Field, new Point(j, i), 20);
        _cellConnections[i, j] = points.Select(r => r.ToArray()).ToArray();
      }
    }
  }

  public void TickUpdate(Context cx)
  {
    CostMap.Clean();

    foreach (var pac in cx.Pacs)
    {
      if (!pac.IsMine)
        continue;
      var points = _cellConnections[pac.Pos];
      var cost = 1f;
      foreach (var bundle in points)
      {
        foreach (var point in bundle)
        {
          CostMap[point] += cost;
        }
        cost -= 0.1f;
        if (cost < 0)
          break;
      }
    }

  }

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
}