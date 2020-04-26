using System.Linq;

class SOrderPushRoadNotOwned : SOrderPushRoad
{
  public SOrderPushRoadNotOwned(Squad owner, Path road) : base(owner, road)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return cx.IsMe(cx.Nodes[Road.Last()].LastOwner) || base.IsCompleted(cx);
  }
}