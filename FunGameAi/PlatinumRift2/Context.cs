using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class Context
{
  public readonly Random Random = new Random();
  // initial
  public int MyId;
  public Node[] Nodes;
  // first tick
  public int TotalPods;
  public Node EnemyHq;
  public Node MyHq;
  public Path SilkRoad;
  // every tick
  public int Tick;
  public int PodsAllocated;

  public readonly List<Squad> Squads = new List<Squad>();

  public Node[] NearestNodes;

  private readonly GameInput _input = new GameInput();

  private int _lastSquadId;

  public int PodsAvailable => MyHq.MyPods - PodsAllocated;

  public void OnInit()
  {
    ReadInitInput();
  }

  public void OnTick()
  {
    ++Tick;
    PodsAllocated = 0;
    ReadTickInput();

    CommitAssetMovement();

    if (Tick == 1)
      InitHq();
  }

  public bool IsEnemy(int playerId) => playerId >= 0 && playerId != MyId;

  public bool IsMe(int? playerId) => playerId.HasValue && playerId == MyId;

  public Squad AddSquad(int nodeId, int pods)
  {
    if (pods <= 0)
      throw new Exception("0 pods");
    if (PodsAllocated + pods > Nodes[nodeId].MyPods)
      throw new Exception($"pods deficite alloc:{PodsAllocated} have {Nodes[nodeId].MyPods} request {pods}");
    var squad = new Squad(++_lastSquadId, nodeId, pods);
    PodsAllocated += pods;
    Squads.Add(squad);
    return squad;
  }

  public void MoveTo(int nodeId, Squad squad)
  {
    Nodes[nodeId].Incomming.Add(squad.Id);

    Console.Write($"{squad.Pods} {squad.NodeId} {nodeId} ");
  }

  private void InitHq()
  {
    EnemyHq = Nodes.First(n => IsEnemy(n.OwnerId));
    MyHq = Nodes.First(n => IsMe(n.OwnerId));
    SilkRoad = Astar.FindPath2(Nodes, MyHq.Id, EnemyHq.Id);

    Astar.CacheDist(Nodes, MyHq.Id, EnemyHq.Id);

    NearestNodes = Nodes.OrderBy(n => n.DistToMyBase).ToArray();

    Player.Print($"path {SilkRoad.Count}" );
  }

  private void ReadInitInput()
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

  private void ReadTickInput()
  {
    int myPlatinum = Int32.Parse(_input.ReadLine()); // your available Platinum
    var totalPods = 0;
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
      node.IsMine = !node.Visible ? node.IsMine : node.OwnerId == MyId;
      node.PlatinumMax = !node.Visible ? node.PlatinumMax : node.Platinum;
      node.LastOwner = !node.Visible ? node.LastOwner : node.OwnerId;
      totalPods += node.MyPods;

      CommitSquadMovement(node);

      if (node.EnemyPods > 0)
        foreach (var n in EnumerateInvisibleConnections(node.Id, 2))
            n.IsMine = false;
    }

    TotalPods = totalPods;
  }

  private void CommitSquadMovement(Node node)
  {
    if (node.Incomming.Count == 0)
      return;
    foreach (var squadId in node.Incomming)
    {
      var squad = Squads.Find(s => s.Id == squadId);
      squad.LastVisited.Enqueue(squad.NodeId);
      if (squad.LastVisited.Count > 8)
        squad.LastVisited.Dequeue();
      squad.NodeId = node.Id;
    }
    node.Incomming.Clear();
  }

  private void CommitAssetMovement()
  {
    var dead = new List<Squad>();

    foreach (var group in Squads.GroupBy(s=>s.NodeId))
    {
      var node = Nodes[group.Key];
      var assignedPods = group.Sum(s => s.Pods);
      if (node.Visible && node.MyPods == assignedPods)
        continue;
      if (!node.Visible || node.MyPods == 0)
      {
        dead.AddRange(group);
      }
      else
      {
        var podsToRemove = assignedPods - node.MyPods;
        foreach (var squad in group)
        {
          var toRemove = Math.Min(squad.Pods, podsToRemove);
          podsToRemove -= toRemove;
          squad.Pods -= toRemove;

          if (squad.Pods == 0)
            dead.Add(squad);
          if (toRemove == 0)
            break;
        }
      }
    }

    foreach (var squad in dead)
    {
      if(!Squads.Remove(squad))
        throw new Exception("shouldn't happen");

      squad.Order.OwnerDie(this);

      foreach (var nodeId in squad.LastVisited)
      {
        var node = Nodes[nodeId];
        if (!node.Visible)
          node.IsMine = false;
      }

      foreach (var node in EnumerateInvisibleConnections(squad.NodeId, 4))
      {
        if (!node.Visible)
          node.IsMine = false;
      }
    }
  }

  private IEnumerable<Node> EnumerateInvisibleConnections(int srcId, int targetDepth)
  {
    var openList = new Queue<int>();
    openList.Enqueue(srcId);
    var closedList = new HashSet<int>();
    var depth = 0;
    var expectedIter = 0;
    for (var i = 0; openList.Count > 0; i++)
    {
      var id = openList.Dequeue();
      closedList.Add(id);

      var node = Nodes[id];

      if (node.Visible)
        continue;

      yield return node;

      if (expectedIter == i)
      {
        ++depth;
        expectedIter = i + node.Connections.Length;
      }

      if (depth > targetDepth)
        continue;

      foreach (var adj in node.Connections)
      {
        if (!closedList.Contains(adj))
        {
          openList.Enqueue(adj);
        }
      }
    }
  }
}