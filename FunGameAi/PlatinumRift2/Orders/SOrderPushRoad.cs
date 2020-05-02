using System;
using System.Linq;

class SOrderPushRoad : SOrderBase
{
  protected readonly Path Road;

  public SOrderPushRoad(Squad owner, Path road) : base(owner)
  {
    Road = road;
  }

  public override bool IsCompleted(Context cx)
  {
    return Owner.NodeId == Road.Last();
  }

  public override bool Execute(Context cx)
  {
    return PushRoad(cx, Road);
  }

  private bool PushRoad(Context cx, Path road)
  {
    // we are fighting
    var fromNode = Owner.NodeId;
    if (cx.Nodes[fromNode].EnemyPods > 0)
      return false;

    var next = road.NextNodeId(fromNode);
    cx.MoveTo(next, Owner);


    return true;
  }
}