public abstract class OrderDig : EOrder
{
  protected Point Pos;

  public OrderDig(Entity robot, Point pos) : base(robot)
  {
    Pos = pos;
  }

  public override string ProduceCommand(Context cx)
  {
    return Entity.Dig(Pos);
  }

  public override void Finalize(Context cx)
  {
    var cell = cx.Field.GetCell(Pos);
    if (Robot.Item == ItemType.Ore && cell.Hole)
      cell.IncreaseDig();
  }
}