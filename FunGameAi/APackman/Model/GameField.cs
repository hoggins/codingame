using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class GameField
{
  public Cell[,] Grid;

  [NonSerialized]
  public List<Point> Gems = new List<Point>();

  [NonSerialized]
  public Map<Point[][]> CellConnections;

  public int Height => Grid.GetLength(0);
  public int Width => Grid.GetLength(1);
  public int Length => Grid.Length;

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
          Flags = row[j] == '#' ? CellFlags.Wall : CellFlags.Space | CellFlags.HadPellet
        };
    }

    InitCellConnections();
  }

  public void ReadTick()
  {
    ResetTick();

    var lostGems = new List<Point>();
    lostGems.AddRange(Gems);

    var visiblePelletCount = int.Parse(GameInput.ReadLine()); // all pellets in sight
    for (var i = 0; i < visiblePelletCount; i++)
    {
      var inputs = GameInput.ReadLine().Split(' ');
      var x = int.Parse(inputs[0]);
      var y = int.Parse(inputs[1]);
      var value = int.Parse(inputs[2]); // amount of points this pellet is worth
      Grid[y,x].SetPellet(value);

      if (value >= 10)
      {
        var point = new Point(x,y);
        if (!Gems.Contains(point))
          Gems.Add(point);
        lostGems.Remove(point);
      }
    }

    foreach (var point in lostGems)
    {
      Gems.Remove(point);
      Grid[point.Y, point.X].SetPellet(0);
    }
  }

  public void ResetTick()
  {
    for (int i = 0; i < Grid.GetLength(0); i++)
    {
      for (int j = 0; j < Grid.GetLength(1); j++)
      {
        Grid[i, j].ResetTick();
      }
    }
  }

  public bool CanTraverse(Point origin)
  {
    return !Grid[origin.Y, origin.X].IsBlocked;
  }

  public int SetPac(Pac pac)
  {
    var f = pac.IsMine ? CellFlags.MyPac : CellFlags.EnemyPac;
    Grid[pac.Pos.Y, pac.Pos.X].SetFlag(f);
    if (Grid[pac.Pos.Y, pac.Pos.X].HasFlag(CellFlags.HadPellet))
      Grid[pac.Pos.Y, pac.Pos.X].ResetFlag(CellFlags.HadPellet);

    if (pac.IsMine)
      return SetVisibleFrom(pac.Pos);
    return 0;
  }

  public CellFlags GetFlags(int y, int x)
  {
    return Grid[y, x].Flags;
  }

  public CellFlags GetFlags(Point p)
  {
    return Grid[p.Y, p.X].Flags;
  }

  public IEnumerable<CellFlags> EnumeratePathFlags(Path path)
  {
    return path.Select(GetFlags);
  }

  public void InitCellConnections()
  {
    CellConnections = new Map<Point[][]>(Height, Width);

    for (int i = 0; i < Height; i++)
    {
      for (int j = 0; j < Width; j++)
      {
        var flags = GetFlags(i,j);
        if (flags.CHasFlag(CellFlags.Wall))
        {
          CellConnections[i, j] = new Point[0][];
          continue;
        }

        var points = AStarUtil.FindNearest(this, new Point(j, i), 20);
        CellConnections[i, j] = points.Select(r => r.ToArray()).ToArray();
      }
    }
  }

  private int SetVisibleFrom(Point pos)
  {
    var visiblePelletCells = 0;
    var sy = (int)pos.Y;
    var sx = (int)pos.X;

    var xIter = Width;
    for (int x = sx; --xIter > 0; x++)
      if (SetVisibleInner(ref x, ref sy, ref visiblePelletCells))
        break;

    for (int x = pos.X; --xIter > 0; x--)
      if (SetVisibleInner(ref x, ref sy, ref visiblePelletCells))
        break;

    var yIter = Height;
    for (int y = sy; --yIter > 0; y++)
      if (SetVisibleInner(ref sx, ref y, ref visiblePelletCells))
        break;

    for (int y = pos.Y; --yIter > 0; y--)
      if (SetVisibleInner(ref sx, ref y, ref visiblePelletCells))
        break;
    return visiblePelletCells;
  }

  private bool SetVisibleInner(ref int x, ref int y, ref int visiblePelletCells)
  {
    var p = new Point(x, y);
    if (AStarUtil.Warp(ref p, Grid.GetLength(1), Grid.GetLength(0)))
    {
      x = p.X;
      y = p.Y;
    }

    if (Grid[y, x].HasFlag(CellFlags.Wall))
      return true;

    if (Grid[y, x].HasFlag(CellFlags.Pellet))
      ++visiblePelletCells;
    else
    {
      if (Grid[y, x].SetPellet(0) || !Grid[y, x].Flags.CHasFlag(CellFlags.Seen))
      {
        WaveUncheck(x, y);
      }
    }

    Grid[y, x].SetFlag(CellFlags.Seen | CellFlags.Visible);
    return false;
  }

  public void WaveUncheck(int x, int y)
  {
    var openList = new List<Point>();
    openList.Add(new Point(x,y));
    var closedList = AStarUtil.GetClosedList(this);
    closedList[new Point(x, y).ToIdx(Width)] = true;

    var adjs = new List<Point>();

    while (openList.Count > 0)
    {
      var toTest = openList[openList.Count-1];
      openList.RemoveAt(openList.Count-1);

      var spaces = 0;

      adjs.Clear();
      for (var i = 0; i < 4; i++)
      {
        var adj = new Point(toTest.X + AStarUtil.ColNum[i], toTest.Y + AStarUtil.RowNum[i]);
        AStarUtil.Warp(ref adj, Width, Height);
        if (!AStarUtil.IsValid(adj, Width, Height)) continue;

        var flags = Grid[adj.Y, adj.X].Flags;

        if (flags.CHasFlag(CellFlags.Wall)) continue;

        if (!closedList[adj.ToIdx(Width)])
        {
          closedList[adj.ToIdx(Width)] = true;
          adjs.Add(adj);
        }

        if (flags.CHasFlag(CellFlags.Pellet))
        {
          spaces = int.MinValue;
          break;
        }
        if (flags.CHasFlag(CellFlags.Space))
          ++spaces;
      }

      if (spaces == 2)
      {
        // tunnel
        //Player.Print($"pred Un {toTest} from ori {x}:{y}");
        Grid[toTest.Y, toTest.X].SetPellet(0);
        openList.AddRange(adjs);
      }
    }


  }

  public float[,] CalcValue()
  {
    var res = new float[Height, Width];
    for (int i = 0; i < Grid.GetLength(0); i++)
    {
      for (int j = 0; j < Grid.GetLength(1); j++)
      {
        var f = Grid[i, j].Flags;
        res[i, j] = Balance.GetCellValue(f);
      }
    }

    return res;
  }

  public void DumpValue()
  {
    for (int i = 0; i < Grid.GetLength(0); i++)
    {
      for (int j = 0; j < Grid.GetLength(1); j++)
      {
        var f = Grid[i, j].Flags;
        Console.Error.Write($"{Balance.GetCellValue(f):0.0} ");
      }
      Console.Error.WriteLine();
    }
  }

  public void Dump()
  {
    for (int i = 0; i < Grid.GetLength(0); i++)
    {
      for (int j = 0; j < Grid.GetLength(1); j++)
      {
        var f = Grid[i, j].Flags;
        if (f.HasFlag(CellFlags.Wall))
        {
          Console.Error.Write("#");
        }
        /*else if (f.HasFlag(CellFlags.Space))
        {
          Console.Error.Write(" ");
        }*/
        else if (f.HasFlag(CellFlags.Pellet))
        {
          Console.Error.Write(".");
        }
        /*else if (f.HasFlag(CellFlags.Visited))
        {
        }*/
        else if (f.HasFlag(CellFlags.EnemyPac))
        {
          Console.Error.Write("e");
        }
        else if (f.HasFlag(CellFlags.MyPac))
        {
          Console.Error.Write("m");
        }
        else if (f.HasFlag(CellFlags.HadPellet))
        {
          Console.Error.Write("x");
        }
        else if (f.HasFlag(CellFlags.Visible))
        {
          Console.Error.Write("v");
        }
        else if (f.HasFlag(CellFlags.Seen))
        {
          Console.Error.Write("o");
        }
        else if ((f & (~CellFlags.Seen)) == f)
        {
          Console.Error.Write("-");
        }
        else
        {
          Console.Error.Write("?");
        }
      }

      Console.Error.WriteLine();
    }
  }

  public Cell this[int y, int x]
  {
    get => Grid[y, x];
    set => Grid[y, x] = value;
  }

  public void CopyFrom(GameField other)
  {Gems = other.Gems;
    CellConnections = other.CellConnections;

    if (Grid == null)
      Grid = new Cell[other.Height, other.Width];

    Array.Copy(other.Grid, Grid, Grid.Length);
  }

  public int Visit(Point p)
  {
    var flags = GetFlags(p);
    var pellets = Grid[p.Y, p.X].PelletCount??0;

    Grid[p.Y, p.X].SetFlag(CellFlags.Seen);
    Grid[p.Y, p.X].SetPellet(0);

    if (pellets > 0)
      return pellets;

    return !flags.CHasFlag(CellFlags.Seen) || flags.CHasFlag(CellFlags.HadPellet)
      ? 1
      : 0;
  }
}