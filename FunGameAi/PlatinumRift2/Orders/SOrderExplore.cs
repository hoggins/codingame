using System.Collections.Generic;
using System.Linq;

class SOrderExplore : SOrderBase
{
  private static readonly Dictionary<int, Path> Cache = new Dictionary<int, Path>();

  private SOrderBase _currentOrder;

  public SOrderExplore(Squad owner) : base(owner)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return false;
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
    n.PlatinumMax.HasValue && !cx.IsMe(n.LastOwner) && n.Incomming.Count == 0;
}