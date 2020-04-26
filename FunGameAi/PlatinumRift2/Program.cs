using System;
using System.Linq;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
  static void Main(string[] args)
  {
    var cx = new Context();
    cx.ReadInitInput();





    // game loop
    Node enemyHq = default;
    Node myHq = default;
    List<int> silkRoad = default;
    bool isSetup = default;
    while (true)
    {
      cx.ReadTickInput();

      // Write an action using Console.WriteLine()
      // To debug: Console.Error.WriteLine("Debug messages...");


      if (!isSetup)
      {
        isSetup = true;
        enemyHq = cx.Nodes.First(n => cx.IsEnemy(n.OwnerId));
        myHq = cx.Nodes.First(n => cx.IsMe(n.OwnerId));
        silkRoad = Astar.FindPath2(cx.Nodes, myHq.Id, enemyHq.Id);

        Player.Print($"path {silkRoad.Count}" );
      }



      var anyCommand = PushRoad(cx.Nodes, silkRoad);


      // first line for movement commands, second line no longer used (see the protocol in the statement for details)
      if (!anyCommand)
        Console.WriteLine("WAIT");
      else
        Console.WriteLine();

      Console.WriteLine("WAIT");
    }
  }

  private static bool PushRoad(Node[] nodes, List<int> silkRoad)
  {
    for (var i = 0; i < silkRoad.Count-1; i++)
    {
      var nodeId = silkRoad[i];
      var node = nodes[nodeId];
      if (node.MyPods == 0)
        continue;
      var next = silkRoad[i + 1];
      Console.Write($"{node.MyPods} {nodeId} {next} ");
    }

    return silkRoad.Count > 2;
  }

  public static void Print(string input)
  {
    Console.Error.WriteLine(input);
  }
}


abstract class GridNode
{
  public int Id;
  public int[] Connections = new int[0];
  public abstract int Value { get; }
}

class Node : GridNode
{
  public int OwnerId;
  public int MyPods;
  public int EnemyPods;
  public bool Visible;
  public int Platinum;
  public override int Value => Platinum;
}


