public class OrderDigOre : OrderDig
{
  public OrderDigOre(Entity robot, (int, int) pos) : base(robot, pos)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    var cell = cx.GetCell(Pos);
    return Robot.Item == ItemType.Ore
           || !cell.Ore.HasValue && cell.Hole
           || cell.Ore.HasValue && cell.Ore == 0
           || !cell.IsSafe();
  }
}