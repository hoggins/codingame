public class OrderMove : EOrder
{
  private readonly (int, int) _pos;

  public OrderMove(Entity robot, (int, int) pos) : base(robot)
  {
    _pos = pos;
  }

  public override bool IsCompleted(Context cx)
  {
    return Robot.X == _pos.Item1 && Robot.Y == _pos.Item2;
  }

  public override string ProduceCommand(Context cx)
  {
    return Entity.Move(_pos);
  }
}