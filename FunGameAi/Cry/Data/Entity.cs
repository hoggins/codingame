using System;

public class Entity
{
  public int Id;
  public EntityType Type;

  public Point Pos;

  public ItemType Item;

  public EOrder Order;

  public int X => Pos.X;
  public int Y => Pos.Y;
  public bool IsDead => Pos == DeadPos;

  private static Point DeadPos = new Point(-1,-1);

  public string Message { get; set; }

  public void Read(string[] inputs)
  {
    Id = int.Parse(inputs[0]); // unique id of the entity
    Type = (EntityType) int.Parse(inputs[1]); // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap
    var x = int.Parse(inputs[2]);
    var y = int.Parse(inputs[3]); // position of the entity
    Item = (ItemType) int.Parse(inputs[4]); // if this entity is a robot, the item it is carrying (-1 for NONE, 2 for RADAR, 3 for TRAP, 4 for ORE)

    Pos = new Point(x, y);
  }

  public static string Move((int, int Y) target)
  {
    return $"MOVE {target.Item1} {target.Item2}";
  }

  public bool IsBusy(Context cx, bool ignoreLowPrio = false)
  {
    if (ignoreLowPrio && Order is OrderRandomDig)
      return false;

    return Order != null && !Order.IsCompleted(cx);
  }

  public static string Dig(Point target)
  {
    return $"DIG {target.X} {target.Y}";
  }

  public static string Take(ItemType item)
  {
    switch (item)
    {
      case ItemType.Radar: return "REQUEST RADAR";
      case ItemType.Trap: return "REQUEST TRAP";
      default:
        throw new ArgumentOutOfRangeException(nameof(item), item, null);
    }
  }

  public string GetOrderName()
  {
    if (Order is OrderChain chain)
      return "ch " + chain.GetOrderName();
    return Order?.GetType().Name ?? "n";
  }

  public bool HasOrder<T>()
  {
    return Order is T || (Order is OrderChain chain) && chain.HasOrder<T>();
  }
  public T GetOrder<T>() where  T : EOrder
  {
    if (Order is T)
      return (T) Order;
    if (Order is OrderChain chain)
      return chain.GetOrder<T>();
    return null;
  }
}