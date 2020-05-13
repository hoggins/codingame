using System;
using System.Collections.Generic;
using System.Linq;

public class GameField
{
  public Cell[,] Grid;

  public readonly List<Point> Gems = new List<Point>();

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
          Flags = row[j] == '#' ? CellFlags.Wall : CellFlags.Space
        };
    }
  }

  public void ReadTick()
  {
    ResetTick();

    var visiblePelletCount = int.Parse(GameInput.ReadLine()); // all pellets in sight
    for (var i = 0; i < visiblePelletCount; i++)
    {
      var inputs = GameInput.ReadLine().Split(' ');
      var x = int.Parse(inputs[0]);
      var y = int.Parse(inputs[1]);
      var value = int.Parse(inputs[2]); // amount of points this pellet is worth
      Grid[y,x].SetPellet(value);

      if (value >= 10)
        Gems.Add(new Point(x,y));
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

    foreach (var gem in Gems)
    {
      Grid[gem.Y, gem.X].ResetFlag(CellFlags.Pellet);
    }
    Gems.Clear();
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

  public CellFlags GetFlags(Point p)
  {
    return Grid[p.Y, p.X].Flags;
  }

  public IEnumerable<CellFlags> EnumeratePathFlags(Path path)
  {
    return path.Select(GetFlags);
  }

  private int SetVisibleFrom(Point pos)
  {
    var visiblePelletCells = 0;
    var sy = (int)pos.Y;
    var sx = (int)pos.X;

    for (int x = sx;; x++)
      if (SetVisibleInner(ref x, ref sy, ref visiblePelletCells))
        break;

    for (int x = pos.X;; x--)
      if (SetVisibleInner(ref x, ref sy, ref visiblePelletCells))
        break;


    for (int y = sy;; y++)
      if (SetVisibleInner(ref sx, ref y, ref visiblePelletCells))
        break;

    for (int y = pos.Y;; y--)
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
      Grid[y, x].SetPellet(0);

    Grid[y, x].SetFlag(CellFlags.Seen | CellFlags.Visible);
    return false;
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

        Grid[i, j].ResetTick();
      }

      Console.Error.WriteLine();
    }
  }
}