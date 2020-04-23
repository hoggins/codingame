public class OrderDigNearestRadar : OrderDigNearest
{
  private readonly int _radarId;

  public OrderDigNearestRadar(Entity robot, (int, int) pos, int radarId) : base(robot, pos)
  {
    _radarId = radarId;
  }

  public override void Finalize(Context cx)
  {
    base.Finalize(cx);
    if (Robot.Item != ItemType.Radar)
    {
      Player.Print("radar placed " + _radarId);
      HighOrderScout.MyRadars.Add((Pos, _radarId));
    }
  }
}