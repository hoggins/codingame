using System;
using System.Collections.Generic;

public class Map
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

  public bool CanTraverse(Point origin)
  {
    return !Grid[origin.Y, origin.X].IsBlocked;
  }



  public IEnumerable<Cell> FindPellet(int minCount)
  {
    for (int i = 0; i < Grid.GetLength(0); i++)
    for (int j = 0; j < Grid.GetLength(1); j++)
      if (Grid[i,j].PelletCount >= minCount)
        yield return Grid[i, j];
  }

  public void SetPac(Pac pac)
  {
    var f = pac.IsMine ? CellFlags.MyPac : CellFlags.EnemyPac;
    Grid[pac.Pos.Y, pac.Pos.X].SetFlag(f);

    if (pac.IsMine)
      SetVisibleFrom(pac.Pos);
  }

  private void SetVisibleFrom(Point pos)
  {
    var sy = pos.Y;
    var sx = pos.X;
    for (int x = sx; x < Grid.GetLength(1); x++)
      if (SetVisibleInner(x, sy))
        break;

    for (int x = pos.X; x >= 0; x--)
      if (SetVisibleInner(x, sy))
        break;

    for (int y = sy; y < Grid.GetLength(0); y++)
      if (SetVisibleInner(sx, y))
        break;

    for (int y = pos.Y; y >= 0; y--)
      if (SetVisibleInner(sx, y))
        break;
  }

  private bool SetVisibleInner(int x, int y)
  {
    if (Grid[y, x].HasFlag(CellFlags.Wall))
      return true;
    Grid[y, x].SetFlag(CellFlags.Seen | CellFlags.Visible);
    return false;
  }
}