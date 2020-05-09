using System;
using System.Collections.Generic;
using System.Linq;

/**
 * Grab the pellets as fast as you can!
 **/
public static class Player
{
  public static void Print(string s) => Console.Error.WriteLine(s);

  private static void Main(string[] args)
  {
    var cx = new Context();
    cx.ReadInit();
    // cx.Map.Dump();

    var ai = new BehTree();

    // game loop
    while (true)
    {
      cx.ReadTick();

      // cx.Map.Dump();

      ai.UpdateTick(cx);
      foreach (var pac in cx.Pacs.Where(p=>p.IsMine))
      {
        ai.UpdateOrder(cx, pac);

        pac.Order?.Execute(cx);
      }

      Console.WriteLine();
    }
  }
}