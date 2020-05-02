using System;
using System.Numerics;

class Context
{
  public Node[] Nodes;
  public Pod[] Pods;

  public void ReadInit()
  {
    var laps = int.Parse(Console.ReadLine());
    var checkpointCount = int.Parse(Console.ReadLine());
    Nodes = new Node[checkpointCount];
    for (var i = 0; i < checkpointCount; i++)
    {
      var inputs = Console.ReadLine().Split(' ');
      int checkpointX = int.Parse(inputs[0]);
      int checkpointY = int.Parse(inputs[1]);
      Nodes[i] = new Node(){Pos = new Vector2(checkpointX, checkpointY)};
    }

    Pods = new Pod[4];
    for (int i = 0; i < 4; i++)
      Pods[i] = new Pod(){IsMine = i < 2};

  }

  public void ReadTick()
  {
    for (int i = 0; i < 4; i++)
    {
      var inputs = Console.ReadLine().Split(' ');
      var pod = Pods[i];
      pod.Pos = new Vector2(int.Parse(inputs[0]),int.Parse(inputs[1]));
      pod.Velocity = new Vector2(int.Parse(inputs[2]),int.Parse(inputs[3]));
      pod.Rotation = int.Parse(inputs[4]);
      pod.NextNodeId = int.Parse(inputs[5]);
    }
  }
}