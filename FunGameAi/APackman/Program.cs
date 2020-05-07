using System;
using System.Linq;

/**
 * Grab the pellets as fast as you can!
 **/
internal static class Player
{
  public static void Print(string s) => Console.Error.WriteLine(s);

  private static void Main(string[] args)
  {

    var cx = new Context();
    cx.ReadInit();

    string[] inputs;
    // game loop
    while (true)
    {
      cx.ReadTick();

      Print(string.Join("\n", cx.Pacs));

      var myPac = cx.Pacs.First(p=>p.IsMine);

      var pellet = cx.Map.FindNearest(myPac.Pos, CellFlags.Pellet);

      myPac.Move(pellet.Value);


    }
  }
}