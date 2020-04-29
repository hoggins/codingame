using System.Collections.Generic;
using System.Linq;

class SOrderChain : SOrderBase
{
  private readonly List<SOrderBase> _orders;

  public SOrderChain(Squad owner, SOrderBase[] orders) : base(owner)
  {
    _orders = orders.ToList();
  }

  public override bool IsCompleted(Context cx)
  {
    return !_orders.Any() || _orders.All(o => o.IsCompleted(cx));
  }

  public override bool Execute(Context cx)
  {
    var subOrder = _orders[0];
    if (!subOrder.IsCompleted(cx))
      return subOrder.Execute(cx);

    // subOrder.Finalize(cx);
    _orders.RemoveAt(0);

    while (_orders.Count > 0)
    {
      var nextOrder = _orders[0];
      if (!nextOrder.IsCompleted(cx))
        return nextOrder.Execute(cx);

      // todo call finalize for skipped orders?
      // subOrder.Finalize(cx);
      _orders.RemoveAt(0);
    }

    return false;
  }

  // public override void Finalize(Context cx)
  // {
  //   base.Finalize(cx);
  //   foreach (var order in _orders)
  //   {
  //     order.Finalize(cx);
  //   }
  // }
}