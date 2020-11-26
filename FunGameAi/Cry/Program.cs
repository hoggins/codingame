using System;
using System.Collections.Generic;

/**
 * Deliver more ore to hq (left side of the map) than your opponent. Use radars to find ore but beware of traps!
 **/
class Player
{
  public static void Print(string s) => Console.Error.WriteLine(s);

  static void Main(string[] args)
  {
    var cx = new Context();
      cx.ReadInitInput();

    // game loop
    while (true)
    {
      cx.TickUpdate();

      // cx.Model.EnemyTracker.Update();

      HighOrderScout.TrySchedule(cx);
      var mineTaken = HighOrderHideScout.TryGive(cx);
      HighOrderScout.TryGive(cx);
      if (!mineTaken)
        HighOrderMine.TryGive(cx);

      foreach (var robot in cx.EnumerateRobots(true))
      {
        if (robot.IsDead)
        {
          Console.WriteLine("WAIT x");
          continue;
        }

        if (!robot.IsBusy(cx))
        {
          robot.Order?.Finalize(cx);

          EOrder newOrder;
          var hasOrder = TryProduceTakeRadar(cx, robot, out newOrder)
                         || TryProduceDigOre(cx, robot, out newOrder)
                         || OrderReturnOre.TryProduce(robot, out newOrder)
                         || OrderRandomDig.TryProduce(cx, robot, out newOrder)
            ;
          robot.Order = newOrder;
        }

        var command = (robot.Order?.ProduceCommand(cx) ?? "WAIT ")
                      + " " + robot.GetOrderName()
          ;
        // WAIT|MOVE x y|DIG x y|REQUEST item
        Console.WriteLine(command);
      }
    }
  }

  public static bool TryProduceTakeRadar(Context cx, Entity robot, out EOrder order)
  {
    order = null;
    return false;
    var unlocked = cx.VisibleOre < Constant.StartRadarSpam || HighOrderScout.MyRadars.Count > 5;
    if (cx.RadarCooldown > 0 || robot.X != 0 || unlocked)
      return false;

    if (robot.Item != ItemType.None)
      return false;

    cx.RadarCooldown = 6;
    order = new OrderTake(robot, ItemType.Radar);
    return true;
  }

  public static bool TryProduceDigOre(Context cx, Entity robot, out EOrder order)
  {
    order = null;
    if (robot.Item == ItemType.Ore)
      return false;
    var oreCell = cx.Field.Map.FindOreBest(robot.Pos);
    if (oreCell == null)
      return false;
    cx.IncDigLock(oreCell.Pos);
    order = new OrderDigOre(robot, oreCell.Pos);
    return true;
  }

}