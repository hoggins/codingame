using System.Collections.Generic;
using System.Linq;

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