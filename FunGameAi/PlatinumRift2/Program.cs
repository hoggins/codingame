using System;
using System.Diagnostics;
using System.Linq;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
  static void Main(string[] args)
  {
    var cx = new Context();
    cx.OnInit();

    // game loop
    var sw = new Stopwatch();

    while (true)
    {
      sw.Restart();
      cx.OnTick();
      SOrderExploreWave.OnTick(cx);

      // Write an action using Console.WriteLine()
      // To debug: Console.Error.WriteLine("Debug messages...");


      var newPods = cx.MyHq.MyPods;
      if (newPods > 0)
      {
        int maxExplorers;
        if (cx.SilkRoad.Count < 4)
          maxExplorers = 0;
        else if (cx.SilkRoad.Count <= 7)
          maxExplorers = 2;
        else
          maxExplorers = 30;
        var explorers = cx.Squads.Count(s => s.Order is SOrderExplore);
/*        // keep def
        newPods -= (int)(2 + cx.TotalPods * 0.1);

        PushExplorers(cx, 3);
        newPods -= 3;

        PushSilkRoad(cx, newPods);*/

        if (newPods == 1)
        {
          if (cx.Squads.Count % 2 == 0 || explorers >= maxExplorers)
            PushSilkRoad(cx, newPods);
          else
            PushExplorers(cx, newPods);
        }
        else
        {
          if (explorers >= maxExplorers)
            PushSilkRoad(cx, newPods);
          else if (cx.Tick == 1)
          {
            var expToSend = Math.Min(newPods, Math.Max(0, maxExplorers - explorers));
            //InitialExplore(cx, expToSend);
            PushExplorers(cx, expToSend);
            PushSilkRoad(cx, cx.PodsAvailable);
          }
          else
          {
            PushSilkRoad(cx, (int) Math.Ceiling(newPods / 2d));
            PushExplorers(cx, (int) Math.Floor(newPods / 2d));
          }
        }
      }

      bool anyCommand = false;
      foreach (var squad in cx.Squads.OrderBy(s=>s.Order is SOrderPushRoad))
      {
        // if (cx.Tick > 1 && sw.ElapsedMilliseconds > 120)
          // break;
        if (squad.Order.IsCompleted(cx))
          squad.Order = new SOrderSeekForEnemy(squad);

        anyCommand |= squad.Order.Execute(cx);
      }



      // first line for movement commands, second line no longer used (see the protocol in the statement for details)
      if (!anyCommand)
        Console.WriteLine("WAIT");
      else
        Console.WriteLine();

      Console.WriteLine("WAIT");
    }
  }

  private static void PushSilkRoad(Context cx, int pods)
  {
    if (pods <= 0)
      return;
    var attackSquad = cx.AddSquad(cx.MyHq.Id, pods);

    // attackSquad.Order = new SOrderPushRoad(attackSquad, cx.SilkRoad);
    attackSquad.Order = new SOrderSeekForEnemy(attackSquad);
  }

  private static void PushExplorers(Context cx, int packs)
  {
    for (int i = 0; i < packs; i++)
    {
      var attackSquad = cx.AddSquad(cx.MyHq.Id, 1);
      // attackSquad.Order = new SOrderExplore(attackSquad);
      attackSquad.Order = new SOrderExploreWave(attackSquad);
    }
  }

  private static void InitialExplore(Context cx, int packs)
  {
    var len = Math.Min(12, cx.SilkRoad.Count);
    var roads = Astar.FindMultiPath(cx.Nodes, cx.MyHq.Id, packs, len);
    var sent = 0;
    foreach (var road in roads)
    {
      sent++;
      if (sent > packs)
        break;

      var attackSquad = cx.AddSquad(cx.MyHq.Id, 1);
      attackSquad.Order =new SOrderChain(attackSquad, new SOrderBase[]
        {
          new SOrderPushRoadNotOwned(attackSquad, road),
          // new SOrderExplore(attackSquad)
          new SOrderExploreWave(attackSquad)
        });
    }
  }


  public static void Print(string input)
  {
    Console.Error.WriteLine(input);
  }
}