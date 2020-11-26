public class OrderDigNearestRadar : OrderDigNearest
{
  public readonly int RadarId;

  public OrderDigNearestRadar(Entity robot, Point pos, int radarId) : base(robot, pos)
  {
    RadarId = radarId;
  }

  public override void Finalize(Context cx)
  {
    base.Finalize(cx);
    if (Robot.Item != ItemType.Radar)
    {
      Player.Print("radar placed " + RadarId);
      HighOrderScout.MyRadars.Add((Pos, RadarId));
    }
  }
}