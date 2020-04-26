using System;

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


    while (true)
    {
      cx.OnTick();

      // Write an action using Console.WriteLine()
      // To debug: Console.Error.WriteLine("Debug messages...");


      var newPods = cx.MyHq.MyPods;
      if (newPods >= 10)
      {
        // keep def
        newPods -= (int)(2 + cx.TotalPods * 0.1);

        PushExplorers(cx, 3);
        newPods -= 3;

        PushSilkRoad(cx, newPods);

        /*if (newPods == 1)
        {
          if (cx.Squads.Count % 2 == 0)
            PushSilkRoad(cx, newPods);
          else
            PushExplore(cx, newPods);
        }
        else
        {
          PushSilkRoad(cx, (int)Math.Ceiling(newPods / 2d));
          PushExplore(cx, (int)Math.Floor(newPods / 2d));
        }*/


      }

      bool anyCommand = false;
      foreach (var squad in cx.Squads)
      {
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
    var attackSquad = cx.AddSquad(cx.MyHq.Id, pods);

    attackSquad.Order = new SOrderPushRoad(attackSquad, cx.SilkRoad);
  }

  private static void PushExplorers(Context cx, int packs)
  {
    for (int i = 0; i < packs; i++)
    {
      var attackSquad = cx.AddSquad(cx.MyHq.Id, 1);
      attackSquad.Order = new SOrderExplore(attackSquad);
    }

  }


  public static void Print(string input)
  {
    Console.Error.WriteLine(input);
  }
}