using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class PlayerPanic
{
  enum CellType { None, Elevator, Obstacle, Exit }

  private struct Cell
  {
    public CellType Type { get; set; }
  }

  class Context
  {
    public Cell[,] Map;
    public int CFloor;
    public int CPos;
    public string CDir;

    public int? LastPos;

    public void ReadInit()
    {
      var inputs = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
      int nbFloors = inputs[0]; // number of floors
      int width = inputs[1]; // width of the area
      int nbRounds = inputs[2]; // maximum number of rounds
      int exitFloor = inputs[3]; // floor on which the exit is found
      int exitPos = inputs[4]; // position of the exit on its floor
      int nbTotalClones = inputs[5]; // number of generated clones
      int nbElevators = inputs[7]; // number of elevators

      Map = new Cell[nbFloors,width];
      Map[exitFloor, exitPos].Type = CellType.Exit;

      for (int i = 0; i < nbElevators; i++)
      {
        inputs = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        int elevatorFloor = inputs[0]; // floor on which this elevator is found
        int elevatorPos = inputs[1]; // position of the elevator on its floor
        Map[elevatorFloor, elevatorPos].Type = CellType.Elevator;
      }
    }

    public void OutWait()
    {
      LastPos = CPos == -1 ? (int?) null : CPos;
      Console.WriteLine("WAIT");
    }

    public void OutBlock()
    {
      Console.WriteLine("BLOCK");
      Map[CFloor, CPos].Type = CellType.Obstacle;
    }

    public void ReadTick()
    {
      var inputs = Console.ReadLine().Split(' ');
      int cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
      int clonePos = int.Parse(inputs[1]); // position of the leading clone on its floor
      string direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT

      CFloor = cloneFloor;
      CPos = clonePos;
      CDir = direction;
    }
  }

  static void Main(string[] args)
  {
    var cx = new Context();
    cx.ReadInit();

    // game loop
    while (true)
    {
      cx.ReadTick();

      if (cx.CDir == "NONE")
      {
        cx.OutWait();
        continue;
      }

      var pos = FindNextExitPoint(cx);
      if (!pos.HasValue && IsSafeToPutBlock(cx))
      {
        cx.OutBlock();
        continue;
      }

      cx.OutWait();

      // Write an action using Console.WriteLine()
      // To debug: Console.Error.WriteLine("Debug messages...");

      // Console.WriteLine("WAIT"); // action: WAIT or BLOCK

    }
  }

public static void Print(string s)=>Console.Error.WriteLine(s);

  private static bool IsSafeToPutBlock(Context cx)
  {
    return cx.LastPos.HasValue;
  }

  private static int? FindNextExitPoint(Context cx)
  {
    foreach (var w in GetCellsInRow(cx.Map, cx.CFloor, cx.CPos, cx.CDir))
    {
      var cell = cx.Map[cx.CFloor, w];
      if (cell.Type != CellType.None)
      {
        Print($"at {w} on {cell.Type}");
        return w;
      }
    }
    return null;
  }

  private static IEnumerable<int> GetCellsInRow(Cell[,] map, int h, int clonePos, string direction)
  {
    switch (direction)
    {
      case "LEFT":
        for (var i = clonePos; i >= 0; i--)
        {
          if (map[h, i].Type != CellType.None)
          {
            Print($"at2 {i} on {map[h, i].Type}");
            yield return i;
            yield break;
          }
          yield return i;
        }
        break;
      case "RIGHT":
        for (var i = clonePos; i < map.GetLength(1); i++)
        {
          if (map[h, i].Type != CellType.None)
          {
            Print($"at3 {i} on {map[h, i].Type}");
            yield return i;
            yield break;
          }
          yield return i;
        }
        break;
    }
  }
}