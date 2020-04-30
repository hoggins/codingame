using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


static class Astar
{

  public static void CacheDist(Node[] map, int myHq, int enemyHq)
  {
    foreach (var pair in CalcAllRoutesLength(map, myHq))
      map[pair.Key].DistToMyBase = pair.Value;
    foreach (var pair in CalcAllRoutesLength(map, enemyHq))
      map[pair.Key].DistToEnemyBase = pair.Value;
  }
  public static Dictionary<int, int> CalcAllRoutesLength(Node[] map, int from, int maxDepth = Int32.MaxValue)
  {
    var routeMap = new Dictionary<int, int>();
    var openList = new Queue<(int from, int to)>();
    openList.Enqueue((from, from));
    routeMap[from] = 1;
    var closedList = new bool[map.Length];
    var nextDepthIdx = 0;
    var curDepth = -1;
    for (int i = 0; openList.Count > 0; i++)
    {
      var src = openList.Dequeue();

      closedList[src.to] = true;

      if (!routeMap.TryGetValue(src.to, out var routeToThis) || routeToThis > routeMap[src.from] + 1)
      {
        routeMap[src.to] = routeMap[src.from] + 1;
      }

      var connections = map[src.to].Connections;
      if (nextDepthIdx == i)
      {
        nextDepthIdx = openList.Count + connections.Length;
        ++curDepth;
      }

      if (curDepth == maxDepth)
        return routeMap;

      foreach (var adj in connections)
      {
        if (!closedList[adj])
          openList.Enqueue((src.to, adj));
      }
    }

    return routeMap;
  }

  public static Path FindPath2(Node[] map, int from, int to)
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

      if (src.to == to)
        return routeMap[src.to];

      foreach (var adj in map[src.to].Connections)
      {
        if (!closedList.Contains(adj))
        {
          openList.Enqueue((src.to, adj));
        }
      }
    }

    return null;
  }

  public static List<Path> FindMultiPath2(Node[] map, int from, int count, int length)
  {
    var routes = new List<Path>();
    var cost = new int[map.Length];

    var allRoutes = CalcAllRoutes(map, from, length).ToList();

    foreach (var path in allRoutes)
    {
      foreach (var nodeId in path)
      {
        ++cost[nodeId];
      }
    }

    for (int i = 0; i < map.Length; i++)
    {
      cost[i] += map[i].DistToEnemyBase;
    }

    allRoutes = allRoutes
      .OrderBy(r => r.Sum(n => cost[n]))
      .Take(count)
      .ToList();
    return allRoutes;

    for (var i = 0; i < count; i++)
    {
      var path = DfsPathWithCost(map, ref cost, from, length);
      routes.Add(path);

      foreach (var id in path)
      {
        ++cost[id];
      }
    }
    return routes;
  }


  public static IEnumerable<Path> CalcAllRoutes(Node[] map, int from, int maxDepth = Int32.MaxValue)
  {
    var deadEnds = new List<int>();
    var routeMap = new Dictionary<int, Path>();
    var openList = new Queue<(int from, int to)>();
    openList.Enqueue((from, from));
    routeMap[from] = new Path(){from};
    var closedList = new bool[map.Length];
    var nextDepthIdx = 0;
    var curDepth = -1;
    for (int i = 0; openList.Count > 0; i++)
    {
      var src = openList.Dequeue();

      closedList[src.to] = true;

      if (!routeMap.TryGetValue(src.to, out var routeToThis) || routeToThis.Count > routeMap[src.from].Count + 1)
      {
        var newRoute = new Path(routeMap[src.from]);
        newRoute.Add(src.to);
        routeMap[src.to] = newRoute;
      }

      var connections = map[src.to].Connections;
      if (nextDepthIdx == i)
      {
        nextDepthIdx = openList.Count + connections.Length;
        ++curDepth;
      }

      if (curDepth == maxDepth)
      {
        deadEnds.Add(src.to);
        continue;
        // return routeMap.Where(p=>deadEnds.Contains(p.Key)).Select(p=>p.Value);
      }

      var isDeadEnd = true;
      foreach (var adj in connections)
      {
        if (!closedList[adj])
        {
          isDeadEnd = false;
          openList.Enqueue((src.to, adj));
        }
      }

      if (isDeadEnd)
        deadEnds.Add(src.to);
    }

    return routeMap.Where(p=>deadEnds.Contains(p.Key)).Select(p=>p.Value);
  }

  public static List<Path> FindMultiPath(Node[] map, int from, int count, int length)
  {
    var routes = new List<Path>();
    var cost = new int[map.Length];

    for (var i = 0; i < count; i++)
    {
      var path = DfsPathWithCost(map, ref cost, from, length);
      routes.Add(path);

      foreach (var id in path)
      {
        ++cost[id];
      }
    }
    return routes;
  }

  private static Path DfsPathWithCost(Node[] map, ref int[] costParam, int @from, int length)
  {
    var cost = costParam;
    var path = new Path();
    var openList = new Stack<(int from, int to)>();
    openList.Push((from, from));
    var closedList = new bool[map.Length];
    while (openList.Count > 0)
    {
      var src = openList.Pop();

      closedList[src.to] = true;
      path.Add(src.to);

      if (path.Count >= length)
        return path;

      foreach (var adj in map[src.to].Connections.OrderBy(c => cost[c]).Take(1))
      {
        if (!closedList[adj])
          openList.Push((src.to, adj));
      }
    }

    return path;
  }


  public static Path FindNearest(Node[] map, int from, Func<Node, bool> predicate)
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

      if (predicate(map[src.to]))
        return routeMap[src.to];

      foreach (var adj in map[src.to].Connections)
      {
        if (!closedList.Contains(adj))
        {
          openList.Enqueue((src.to, adj));
        }
      }
    }

    return null;
  }
}