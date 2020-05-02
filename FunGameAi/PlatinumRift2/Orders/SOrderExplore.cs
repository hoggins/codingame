using System;
using System.Collections.Generic;
using System.Linq;

class SOrderExploreWave : SOrderBase
{
  private Path _curPath;
  private bool _forceTravers;

  public SOrderExploreWave(Squad owner) : base(owner)
  {
  }

  public static void OnTick(Context cx)
  {
    for (int i = 0; i < cx.PathMap.Length; i++)
    {
      if (cx.PathPlague[i] > 0)
        ++cx.PathPlague[i];
      cx.PathMap[i] -= 1;
    }
  }

  public override bool IsCompleted(Context cx)
  {
    return false;
  }

  public override bool Execute(Context cx)
  {
    var curNode = cx.Nodes[Owner.NodeId];
    var best = GetInstantCaptureNode(cx, Owner, curNode);
    if (best != null)
    {
      var bestNode = cx.Nodes[best.Value];
      if (bestNode.EnemyPods > 0 && bestNode.EnemyPods <= Owner.Pods && bestNode.Platinum <= curNode.Platinum)
        return false;
      _curPath = null;
      RegisterPath(cx, new Path(){curNode.Id, bestNode.Id});
    }
    else
    {
      if (CanTraversePath(cx))
        best = _curPath.NextNodeId(Owner.NodeId);
      else
      {
        // do not use cached dist - it causes units to return back. Probably find a way weight owned territory
        _curPath = AStarUtil.FindPathDirection(cx.Nodes, cx.DistMapFromMe, cx.PathMap, cx.PathPlague, Owner.NodeId, 4);
        if (_curPath.Count == 1)
          return false;
        _forceTravers = !CanTraversePath(cx);
        if (_forceTravers)
          throw new Exception("why ? " + _curPath + " " + cx.Nodes[_curPath.Last()].IsMine);
        RegisterPath(cx, _curPath);
        best = _curPath[1];
      }
    }

    if (!best.HasValue)
      throw new Exception("no explore action");

    cx.MoveTo(best.Value, Owner);
    return true;
  }

  private bool CanTraversePath(Context cx)
  {
    if (_curPath == null)
      return false;

    var last = _curPath.Last();
    if (_forceTravers)
      return last != Owner.NodeId;

    return last != Owner.NodeId && !cx.Nodes[last].IsMine;
  }

  private void RegisterPath(Context cx, Path path)
  {
    Player.Print($"explore {path}");
    for (var pI = 1; pI < path.Count; pI++)
    {
      //AStarUtil.IncreaseNeighborsCost(cx.Nodes, cx.PathMap, path, pI, 1);
      var id = path[pI];
      cx.PathMap[id] += 1;
    }
  }

  private int? GetInstantCaptureNode(Context cx, Squad owner, Node curNode)
  {
    var best = curNode.Connections
      .Select(c => cx.Nodes[c])
      .FindMax(n => !n.IsMine && n.Incomming.Count == 0 ? n.Platinum : Int32.MinValue);
    return best.IsMine || best.Platinum == 0 || best.Incomming.Count > 0 ? (int?) null : best.Id;
  }
}

class SOrderExplore : SOrderBase
{
  private static readonly Dictionary<int, Path> Cache = new Dictionary<int, Path>();

  private SOrderBase _currentOrder;
  private bool _completed;

  public SOrderExplore(Squad owner) : base(owner)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return _completed;
  }

  public override bool Execute(Context cx)
  {
    if (_currentOrder != null && !_currentOrder.IsCompleted(cx))
      return _currentOrder.Execute(cx);

    if (_currentOrder == null || _currentOrder is SOrderPushRoadNotOwned)
    {
      _currentOrder = new SOrderSeekForPlatinum(Owner);
      if (!_currentOrder.IsCompleted(cx))
        return _currentOrder.Execute(cx);
    }

    var path = FindNearestToCapture(cx, Owner.NodeId);

    if (path == null)
    {
      _completed = true;
      return false;
    }

    _currentOrder = new SOrderPushRoadNotOwned(Owner, path);
    return _currentOrder.Execute(cx);
  }

  private static Path FindNearestToCapture(Context cx, int from)
  {
    if (Cache.TryGetValue(from, out var path) && ShouldCapture(cx, cx.Nodes[path.Last()]))
      return path;
    path = Astar.FindNearest(cx.Nodes, from, n=>ShouldCapture(cx, n));
    Cache[from] = path;
    return path;
  }

  public static bool ShouldCapture(Context cx, Node n) =>
    (!n.PlatinumMax.HasValue || !n.IsMine && n.PlatinumMax > 1) && n.Incomming.Count == 0;
}