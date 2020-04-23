using System;

public class Entity
{
  public int Id;
  public EntityType Type;
  public int X;
  public int Y;
  public ItemType Item;

  public EOrder Order;

  public bool IsDead => X == -1 && Y == -1;
  public (int, int) Pos => (X, Y);
  public string Message { get; set; }

  public void Read(string[] inputs)
  {
    Id = int.Parse(inputs[0]); // unique id of the entity
    Type = (EntityType) int.Parse(inputs[1]); // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap
    X = int.Parse(inputs[2]);
    Y = int.Parse(inputs[3]); // position of the entity
    Item = (ItemType) int.Parse(
      inputs[4]); // if this entity is a robot, the item it is carrying (-1 for NONE, 2 for RADAR, 3 for TRAP, 4 for ORE)
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

  public static string Dig((int, int) target)
  {
    return $"DIG {target.Item1} {target.Item2}";
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
}