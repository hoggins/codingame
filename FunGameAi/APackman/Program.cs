using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/**
 * Grab the pellets as fast as you can!
 **/
public static class Player
{
  public static void Print(string s)
  {
    // return;
    Console.Error.WriteLine(s);
  }

  private static void Main(string[] args)
  {
    var cx = new Context();
    cx.ReadInit();
    // cx.Map.Dump();

    var ai = new BehTree();

    var sw = Stopwatch.StartNew();
    // game loop
    while (true)
    {
      sw.Restart();

      cx.ReadTick();

      //var nearest = cx.Map.FindBestPath(cx.Pacs.FindMin(p=>p.IsMine?p.Id : Int32.MaxValue).Pos, 10, 10);

      // cx.Map.Dump();

      TrafficLight.UpdateTick(cx);
      ai.UpdateTick(cx);
      var anyOut = false;
      foreach (var pac in cx.Pacs.Where(p=>p.IsMine))
      {
        ai.UpdateOrder(cx, pac);

        anyOut |= pac.Order?.Execute(cx) == true;
      }

      Print($"{sw.ElapsedMilliseconds} ms");
      Console.WriteLine();
    }
  }
}