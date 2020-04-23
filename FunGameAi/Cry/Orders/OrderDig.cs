public abstract class OrderDig : EOrder
{
  protected (int, int) Pos;

  public OrderDig(Entity robot, (int, int) pos) : base(robot)
  {
    Pos = pos;
  }

  public override string ProduceCommand(Context cx)
  {
    return Entity.Dig(Pos);
  }

  public override void Finalize(Context cx)
  {
    var cell = cx.GetCell(Pos);
    if (Robot.Item == ItemType.Ore && cell.Hole)
      cell.IncreaseDig();
  }
}