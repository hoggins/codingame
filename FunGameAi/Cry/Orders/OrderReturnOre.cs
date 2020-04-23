public class OrderReturnOre : EOrder
{
  public readonly (int, int Y) Target;

  private OrderReturnOre(Entity robot, (int, int Y) target) : base(robot)
  {
    Target = target;
  }

  public static bool TryProduce(Entity robot, out EOrder newOrder)
  {
    newOrder = null;
    if (robot.Item != ItemType.Ore)
      return false;
    newOrder = new OrderReturnOre(robot, (0, robot.Y));
    return true;
  }

  public override bool IsCompleted(Context map)
  {
    return Robot.Item == ItemType.None;
  }

  public override string ProduceCommand(Context cx)
  {
    return Entity.Move(Target);
  }
}