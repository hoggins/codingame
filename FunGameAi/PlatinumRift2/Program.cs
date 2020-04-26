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
    var attackSquad = new Squad();

    while (true)
    {
      cx.OnTick();

      // Write an action using Console.WriteLine()
      // To debug: Console.Error.WriteLine("Debug messages...");

      if (attackSquad.Order == null)
        attackSquad.Order = new SOrderPushRoad(attackSquad, cx.SilkRoad);

      if (cx.MyHq.MyPods > 0)
        cx.AddAsset(attackSquad, cx.MyHq.Id, cx.MyHq.MyPods);

      var anyCommand = attackSquad.Order.Execute(cx);


      // first line for movement commands, second line no longer used (see the protocol in the statement for details)
      if (!anyCommand)
        Console.WriteLine("WAIT");
      else
        Console.WriteLine();

      Console.WriteLine("WAIT");
    }
  }



  public static void Print(string input)
  {
    Console.Error.WriteLine(input);
  }
}