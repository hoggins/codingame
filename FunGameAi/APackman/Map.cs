using System;
using System.Collections.Generic;

internal class Map
{
  public Cell[,] Grid;

  public void ReadInit()
  {
    string[] inputs;
    inputs = GameInput.ReadLine().Split(' ');
    var width = int.Parse(inputs[0]); // size of the grid
    var height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)
    Grid = new Cell[height,width];
    for (ushort i = 0; i < height; i++)
    {
      var row = GameInput.ReadLine(); // one line of the grid: space " " is floor, pound "#" is wall
      for (ushort j = 0; j < row.Length; j++)
        Grid[i,j] = new Cell
        {
          Pos = new Point(j,i),
          Flags = row[j] == ' ' ? CellFlags.Space : CellFlags.Wall
        };
    }
  }

  public void ReadTick()
  {
    for (int i = 0; i < Grid.GetLength(0); i++)
    {
      for (int j = 0; j < Grid.GetLength(1); j++)
      {
        Grid[i,j].ResetTick();
      }
    }

    var visiblePelletCount = int.Parse(GameInput.ReadLine()); // all pellets in sight
    for (var i = 0; i < visiblePelletCount; i++)
    {
      var inputs = GameInput.ReadLine().Split(' ');
      var x = int.Parse(inputs[0]);
      var y = int.Parse(inputs[1]);
      var value = int.Parse(inputs[2]); // amount of points this pellet is worth
      Grid[y,x].SetPellet(value);
    }
  }

  public Point? FindNearest(Point pos,  CellFlags flags)
  {
    var openList = MakeList(pos, true);
    var nextOpenList = MakeList(pos, false);

    var visited = new HashSet<int>();

    for (var i = 0; ; i++)
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
      if (!CanTraverse(c))
        continue;
      if (Grid[c.Y, c.X].Flags.HasFlag(flags))
        return c;
      FillNeighbors(c, visited, nextOpenList);
      FillPortal(Grid, c, visited, nextOpenList);
    }
  }

  private bool CanTraverse(Point origin)
  {
    if (origin.Y >= Grid.GetLength(0)
        || origin.X >= Grid.GetLength(1))
      return false;
    var cell = Grid[origin.Y, origin.X];
    return !cell.IsBlocked;
  }

  private void FillNeighbors(Point origin, HashSet<int> visited, List<Point> res)
  {
    void TryAdd(Point p)
    {
      if (visited.Add((p.X << 16) + (p.Y)))
        res.Add(p);
    }

    TryAdd(new Point(origin.X-1, origin.Y));
    TryAdd(new Point(origin.X, origin.Y+1));
    TryAdd(new Point(origin.X+1, origin.Y));
    TryAdd(new Point(origin.X, origin.Y-1));
  }

  private void FillPortal(Cell[,] grid, Point point, HashSet<int> visited, List<Point> res)
  {
    void TryAdd(Point p)
    {
      if (visited.Add((p.X << 16) + (p.Y)))
        res.Add(p);
    }

    var lastX = grid.GetLength(1)-1;
    if (point.X == 0)
      TryAdd(new Point(lastX,point.Y));
    if (point.X == lastX)
      TryAdd(new Point(0, (int)point.Y));

    var lastY = grid.GetLength(0)-1;
    if (point.Y == 0)
      TryAdd(new Point(point.X, lastY));
    if (point.Y == lastY)
      TryAdd(new Point((int)point.X, 0));
  }

  private List<T> MakeList<T>(T proto, bool add)
  {
    var list = new List<T>();
    if (add)
      list.Add(proto);
    return list;
  }
}