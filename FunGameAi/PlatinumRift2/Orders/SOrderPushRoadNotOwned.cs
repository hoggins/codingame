using System.Linq;

class SOrderPushRoadNotOwned : SOrderPushRoad
{
  public SOrderPushRoadNotOwned(Squad owner, Path road) : base(owner, road)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    var target = cx.Nodes[Road.Last()];
    return target.IsMine || target.Incomming.Count > 0 || base.IsCompleted(cx);
  }
}