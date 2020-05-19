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
      cx.ResetTick();
      ++cx.Tick;
      ReadTickInput(cx);
      cx.PatchMap();

      cx.Model.EnemyTracker.Update();

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

  #region Input


  private static void ReadTickInput(Context cx)
  {
    string[] inputs;
    inputs = Console.ReadLine().Split(' ');
    cx.MyScore = int.Parse(inputs[0]); // Amount of ore delivered
    cx.OpponentScore = int.Parse(inputs[1]);
    InputReadMap(cx, cx.Field.Map.GetLength(1), cx.Field.Map.GetLength(0));

    inputs = Console.ReadLine().Split(' ');
    int entityCount = int.Parse(inputs[0]); // number of entities visible to you
    cx.RadarCooldown = int.Parse(inputs[1]); // turns left until a new radar can be requested
    cx.TrapCooldown = int.Parse(inputs[2]); // turns left until a new trap can be requested
    var updated = new HashSet<int>();
    for (int i = 0; i < entityCount; i++)
    {
      inputs = Console.ReadLine().Split(' ');
      var id = int.Parse(inputs[0]); // unique id of the entity
      var entity = cx.Entities.Find(e => e.Id == id);
      if (entity == null)
        cx.Entities.Add(entity = new Entity());
      entity.Read(inputs);
      updated.Add(entity.Id);
    }
    cx.Entities.RemoveAll(e => !updated.Contains(e.Id));
  }

  private static void InputReadMap(Context cx, int height, int width)
  {
    string[] inputs;
    for (int i = 0; i < height; i++)
    {
      inputs = Console.ReadLine().Split(' ');
      for (int j = 0; j < width; j++)
      {
        int? ore = inputs[2 * j] == "?" ? (int?) null : int.Parse(inputs[2 * j]); // amount of ore or "?" if unknown
        bool hole = int.Parse(inputs[2 * j + 1]) == 1; // 1 if cell has a hole
        cx.Field.Map[j, i].Set(ore, hole);
      }
    }
  }

  #endregion

}