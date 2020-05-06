using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerOfThor
{
  class Node
  {
    public int Id;
    public int[] Connections = new int[0];
    public bool IsGateway;
    public bool IsClosed;
  }

  public class GraphTest
  {
    static void MainGraph(string[] args)
    {
      string[] inputs;
      inputs = Console.ReadLine().Split(' ');
      int N = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
      var nodes = new Node[N];
      for (int i = 0; i < N; i++)
        nodes[i] = new Node();
      int L = int.Parse(inputs[1]); // the number of links
      int E = int.Parse(inputs[2]); // the number of exit gateways
      for (int i = 0; i < L; i++)
      {
        inputs = Console.ReadLine().Split(' ');
        int zone1 = Int32.Parse(inputs[0]);
        int zone2 = Int32.Parse(inputs[1]);
        nodes[zone1].Connections = nodes[zone1].Connections.Union(new[] {zone2}).ToArray();
        nodes[zone2].Connections = nodes[zone2].Connections.Union(new[] {zone1}).ToArray();
      }
      for (int i = 0; i < E; i++)
      {
        int EI = int.Parse(Console.ReadLine()); // the index of a gateway node
        nodes[EI].IsGateway = true;
      }

      // game loop
      while (true)
      {
        int SI = int.Parse(Console.ReadLine()); // The index of the node on which the Skynet agent is positioned this turn

        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");


        var path = FindNearest(nodes, SI);

        // Example: 0 1 are the indices of the nodes you wish to sever the link between
        Console.WriteLine(path[0] + " " + path[1]);
      }
    }

    class Path : List<int>
    {
      public Path()
      {
      }

      public Path(int capacity) : base(capacity)
      {
      }

      public Path(IEnumerable<int> collection) : base(collection)
      {
      }
    }

    private static Path FindNearest(Node[] map, int from)
    {
      var routeMap = new Dictionary<int, Path>();
      var openList = new Queue<(int from, int to)>();
      openList.Enqueue((from, from));
      routeMap[from] = new Path(){from};
      var closedList = new HashSet<int>();
      while (openList.Count > 0)
      {
        var src = openList.Dequeue();

        closedList.Add(src.to);

        if (!routeMap.TryGetValue(src.to, out var routeToThis) || routeToThis.Count > routeMap[src.from].Count + 1)
        {
          var newRoute = new Path(routeMap[src.from]);
          newRoute.Add(src.to);
          routeMap[src.to] = newRoute;
        }

        if (map[src.to].IsGateway)
          return routeMap[src.to];

        foreach (var adj in map[src.to].Connections)
        {
          if (map[adj].IsClosed)
            continue;
          if (!closedList.Contains(adj))
          {
            openList.Enqueue((src.to, adj));
          }
        }
      }

      return null;
    }
  }
}