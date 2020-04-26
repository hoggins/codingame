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
    if (cx.Nodes[Owner.NodeId].EnemyPods > 0)
      return false;

    var curIdx = road.FindIndex(Owner.NodeId);
    if (curIdx == road.Count - 1)
      return false;

    var next = road[curIdx + 1];
    cx.MoveTo(next, Owner);


    return true;
  }
}