using System;
using System.Linq;

class Context
{
  public int MyId;
  public Node[] Nodes;

  private readonly GameInput _input = new GameInput();

  public void ReadInitInput()
  {
    string[] inputs;
    inputs = _input.ReadLine().Split(' ');
    int playerCount = Int32.Parse(inputs[0]); // the amount of players (always 2)
    MyId = Int32.Parse(inputs[1]); // my player ID (0 or 1)
    int zoneCount = Int32.Parse(inputs[2]); // the amount of zones on the map
    Nodes = new Node[zoneCount];
    int linkCount = Int32.Parse(inputs[3]); // the amount of links between all zones
    for (int i = 0; i < zoneCount; i++)
    {
      inputs = _input.ReadLine().Split(' ');
      int zoneId = Int32.Parse(inputs[0]); // this zone's ID (between 0 and zoneCount-1)
      int platinumSource = Int32.Parse(inputs[1]); // Because of the fog, will always be 0
      Nodes[zoneId] = new Node {Id = i, Platinum = platinumSource};
    }

    for (int i = 0; i < linkCount; i++)
    {
      inputs = _input.ReadLine().Split(' ');
      int zone1 = Int32.Parse(inputs[0]);
      int zone2 = Int32.Parse(inputs[1]);
      Nodes[zone1].Connections = Nodes[zone1].Connections.Union(new[] {zone2}).ToArray();
      Nodes[zone2].Connections = Nodes[zone2].Connections.Union(new[] {zone1}).ToArray();
    }
  }

  public void ReadTickInput()
  {
    int myPlatinum = Int32.Parse(_input.ReadLine()); // your available Platinum
    for (int i = 0; i < Nodes.Length; i++)
    {
      var inputs = _input.ReadLine().Split(' ');
      var node = Nodes[i];
      int zId = Int32.Parse(inputs[0]); // this zone's ID
      node.OwnerId = Int32.Parse(inputs[1]); // the player who owns this zone (-1 otherwise)
      var podsP0 = Int32.Parse(inputs[2]); // player 0's PODs on this zone
      var podsP1 = Int32.Parse(inputs[3]); // player 1's PODs on this zone
      node.Visible = Int32.Parse(inputs[4]) == 1; // 1 if one of your units can see this tile, else 0
      node.Platinum= Int32.Parse(inputs[5]); // the amount of Platinum this zone can provide (0 if hidden by fog)

      node.MyPods = MyId == 0 ? podsP0 : podsP1;
      node.EnemyPods = MyId == 0 ? podsP1 : podsP0;
    }
  }

  public bool IsEnemy(int playerId) => playerId >= 0 && playerId != MyId;
  public bool IsMe(int playerId) => playerId == MyId;
}