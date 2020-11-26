using System.Collections.Generic;
using System.Linq;

public class OrderChain : EOrder
{
  private readonly List<EOrder> _orders;

  public EOrder FirstOrder => _orders[0];

  public OrderChain(Entity robot, EOrder[] orders) : base(robot)
  {
    _orders = orders.ToList();
  }

  public override bool IsCompleted(Context cx)
  {
    return !_orders.Any() || _orders.All(o => o.IsCompleted(cx));
  }

  public override string ProduceCommand(Context cx)
  {
    var subOrder = _orders[0];
    if (!subOrder.IsCompleted(cx))
      return subOrder.ProduceCommand(cx);

    subOrder.Finalize(cx);
    _orders.RemoveAt(0);

    while (_orders.Count > 0)
    {
      var nextOrder = _orders[0];
      if (!nextOrder.IsCompleted(cx))
        return nextOrder.ProduceCommand(cx);

      // todo call finalize for skipped orders?
      subOrder.Finalize(cx);
      _orders.RemoveAt(0);
    }

    return null;
  }

  public override void Finalize(Context cx)
  {
    base.Finalize(cx);
    foreach (var order in _orders)
    {
      order.Finalize(cx);
    }
  }

  public string GetOrderName()
  {
    return _orders.FirstOrDefault()?.GetType().Name ?? "n";
  }

  public bool HasOrder<T>()
  {
    return _orders.Any(o=>o is T);
  }
  public T GetOrder<T>() where T : EOrder
  {
    return (T) _orders.FirstOrDefault(o=>o is T);
  }
}