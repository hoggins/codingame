using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[Serializable]
public class TheOut
{
  public float[,] Values;
  public float[,] Infl;
  public List<Pac> Pacs;
}

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

  class ConsoleWriter
  {
    private readonly string _str;
    private int _i;
    public bool IsDone;

    public ConsoleWriter(string str)
    {
      _str = str;
      _i = 0;
    }

    public void Tick()
    {
      Print(new string(_str.Skip(_i*4000).Take(4000).ToArray()));
      ++_i;
      IsDone = _i * 4000 > _str.Length;

      if (IsDone)
        Print("\n\ndone: " + _str.Length);
    }
  }

  private static ConsoleWriter _cw;

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
      cx.UpdateTick();

      // if (cx.Pacs.Any(p=>p.Type == PacType.Dead))
        // throw new Exception("why \n" + string.Join("\n", cx.Pacs));

      //var nearest = cx.Map.FindBestPath(cx.Pacs.FindMin(p=>p.IsMine?p.Id : Int32.MaxValue).Pos, 10, 10);

      // cx.Field.Dump();
      // Print("\nfield\n");
      // cx.Field.DumpValue();
      // Print("\ncost\n");
      // cx.Infl.CostMap.Dump(c=>$"{c:0.0} ");

      /*if (_cw == null || _cw.IsDone)
      {
        var theOut = new TheOut
        {
          Values = cx.Field.CalcValue(),
          Infl = cx.Infl.CostMap.ToArray(),
          Pacs = cx.Pacs,
        };
        cx.Writer.Write(theOut);
        _cw = new ConsoleWriter(cx.Writer.Flush());
      }
      _cw?.Tick();*/



      TrafficLight.UpdateTick(cx);
      ai.UpdateTick(cx);

      // Profile(cx);
      // return;

      foreach (var pac in cx.Pacs.Where(p => p.IsMine))
      {
        ai.UpdateOrder(cx, pac);
      }

      ai.UpdatePostTick(cx);

      var anyOut = false;
      foreach (var pac in cx.Pacs.Where(p=>p.IsMine))
      {
        var hasOut = pac.Order?.Execute(cx) == true;
        anyOut |= hasOut;

        pac.IsInClutch = false;
        // Print($"RE: {pac} ai:{pac.Order?.GetType().Name} hasOut:{hasOut}");
      }

      Print($"{sw.ElapsedMilliseconds} ms");
      Console.WriteLine();
    }
  }

  private static void Profile(Context cx)
  {
    foreach (var pac in cx.Pacs)
    {
      var best = cx.Field.FindBestPath(pac.Pos, 10, 10);
      Console.WriteLine(best.Count);
    }
  }
}