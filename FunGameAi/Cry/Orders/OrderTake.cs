public class OrderTake : EOrder
{
  private readonly ItemType _item;

  public OrderTake(Entity robot, ItemType item) : base(robot)
  {
    _item = item;
  }

  public override bool IsCompleted(Context cx)
  {
    return Robot.Item == _item;
  }

  public override string ProduceCommand(Context cx)
  {
    return Entity.Take(_item);
  }
}