using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

static class AStarUtil
{
  public static Path FindPathDirection(Node[] map, int[] distMap, int[] pathCost, int[] cxPathPlague, int origin,
    int length)
  {
    var cameFrom = new Dictionary<int, (int fromNode, int len)>();
    var openList = new HashSet<int> {origin};
    var closedList = new bool[map.Length];

    for (var i = 0; openList.Count > 0; i++)
    {
      var src = openList.FindMin(n=>distMap[n] + pathCost[n] + cxPathPlague[n] + (map[n].IsMine ? 10 : 0));
      openList.Remove(src);
      closedList[src] = true;

      if (!cameFrom.TryGetValue(src, out var srcInfo))
        srcInfo = (-1, 0);

      // if (srcInfo.len >= length)
        // return ReconstructPath(cameFrom, src,srcInfo.len);

        var node = map[src];
        if (!node.IsMine && node.Incomming.Count == 0)
          return ReconstructPath(cameFrom, src,srcInfo.len);

      var connections = node.Connections;
      // if (connections.Length == 1 && srcInfo.len > 1)
        // return ReconstructPath(cameFrom, src,srcInfo.len);

      foreach (var adj in connections)
      {
        if (!closedList[adj] && openList.Add(adj))
          cameFrom[adj] = (src, srcInfo.len + 1);
      }
    }

    return null;
  }

  public static void IncreaseNeighborsCost(Node[] map, int[] cost, Path path, int i, int addCost)
  {
    var (pNode, nNode) = (-1,-1);
    if (i > 1)
      pNode = path[i - 1];
    if (i + 1 < path.Count)
      nNode = path[i + 1];
    foreach (var adj in map[i].Connections)
    {
      if (adj == pNode || adj == nNode)
        continue;
      cost[adj] = addCost;
    }
  }

  private static Path ReconstructPath(Dictionary<int, (int fromNode, int len)> cameFrom, int src, int depth)
  {
    var path = new Path(depth);
    path.Depth = depth;
    path.Add(src);
    while (cameFrom.ContainsKey(src))
    {
      src = cameFrom[src].fromNode;
      path.Add(src);
    }

    path.Reverse();
    return path;
  }
}


static class Astar
{

  public static Dictionary<int, Dictionary<int, Path>> AllRoutesCache = new Dictionary<int, Dictionary<int, Path>>();

  public static void CacheDist(Node[] map, int myHq, int enemyHq)
  {
    {
      var allRoutes = CalcAllRoutes(map, myHq, false).ToList();
      foreach (var path in allRoutes)
        map[path.Last()].DistToMyBase = path.Count;
      AllRoutesCache.Add(myHq, allRoutes.ToDictionary(p=>p.Last()));
    }
    {
      var allRoutes = CalcAllRoutes(map, enemyHq, false).ToList();
      foreach (var path in allRoutes)
        map[path.Last()].DistToEnemyBase = path.Count;
      AllRoutesCache.Add(enemyHq, allRoutes.ToDictionary(p=>p.Last()));
    }
  }

  public static Path FindPath2(Node[] map, int from, int to)
  {
    if (AllRoutesCache.TryGetValue(from, out var cBundle) && cBundle.TryGetValue(to, out var cPath))
      return cPath;

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

  public static IEnumerable<Path> CalcAllRoutes(Node[] map, int from, bool onlyRoots, int maxDepth = Int32.MaxValue)
  {
    if (AllRoutesCache.TryGetValue(from, out var cBundle))
      return cBundle.Values;

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
        newRoute.Depth = curDepth;
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

    if (onlyRoots)
      return routeMap.Where(p=>deadEnds.Contains(p.Key)).Select(p=>p.Value);
    return routeMap.Select(p=>p.Value);
  }

  public static List<Path> FindMultiPath(Node[] map, int from, int count, int length)
  {
    var routes = new List<Path>();
    var distCost = new int[map.Length];
    var pathCost = new int[map.Length];

    for (int i = 0; i < map.Length; i++)
      distCost[i] += map[i].DistToEnemyBase;

    for (var i = 0; i < count; i++)
    {
      var path = AStarUtil.FindPathDirection(map, distCost, pathCost, distCost, @from, length);
      routes.Add(path);

      Player.Print("tn " + path + " d" + path.Depth);

      for (var pI = 1; pI < path.Count; pI++)
      {
        AStarUtil.IncreaseNeighborsCost(map, pathCost, path, pI, 1);
        var id = path[pI];
        pathCost[id] = 1;
      }
    }
    return routes;
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