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
  public static Dictionary<int, int> CalcAllRoutesLength(Node[] map, int from)
  {
    var routeMap = new Dictionary<int, int>();
    var openList = new Queue<(int from, int to)>();
    openList.Enqueue((from, from));
    routeMap[from] = 1;
    var closedList = new bool[map.Length];
    while (openList.Count > 0)
    {
      var src = openList.Dequeue();

      closedList[src.to] = true;

      if (!routeMap.TryGetValue(src.to, out var routeToThis) || routeToThis > routeMap[src.from] + 1)
      {
        routeMap[src.to] = routeMap[src.from] + 1;
      }

      foreach (var adj in map[src.to].Connections)
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

class Path : List<int>
{
  public Path()
  {
  }

  public Path(Path other)
  :base (other as List<int>)
  {
  }

  public int FindIndex(int targetId)
  {
    for (var i = 0; i < Count; i++)
    {
      var nodeId = this[i];
      if (nodeId == targetId)
        return i;
    }

    return -1;
  }

  public override string ToString()
  {
    return string.Join(" ", this);
  }
}