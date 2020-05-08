using System;
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

    // game loop
    while (true)
    {
      cx.ReadTick();

      if (cx.Tick == 1)
      {
        var pellets = cx.Map.FindPellet(10).ToList();
        foreach (var pellet in pellets)
        {
          Print("Has pellet " + pellet);
          var best = ((Pac pac, Path path)?) null;
          foreach (var pac in cx.Pacs)
          {
            var path = cx.Map.FindPath(pac.Pos, pellet.Pos);
            if (!best.HasValue || best.Value.path.Count > path.Count)
              best = (pac, path);
          }

          if (best.HasValue)
          {
            Print("pac " + best.Value.pac);
            Print("path " + best.Value.path);
            best.Value.pac.Order = new POrderMoveTo(best.Value.pac, pellet.Pos);
          }
        }
      }


      foreach (var pac in cx.Pacs.Where(p=>p.IsMine))
      {
        if (pac.Order == null || pac.Order.IsCompleted(cx))
        {
          var pellet = cx.Map.FindNearest(pac.Pos, CellFlags.Pellet);
          if (pellet.HasValue)
            pac.Order = new POrderMoveTo(pac, pellet.Value);
        }

        pac.Order?.Execute(cx);
      }

      Console.WriteLine();
    }
  }
}

public abstract class POrderBase
{
  protected readonly Pac Owner;

  protected POrderBase(Pac owner)
  {
    Owner = owner;
  }

  public abstract bool IsCompleted(Context cx);

  public abstract bool Execute(Context cx);
}

public class POrderMoveTo : POrderBase
{
  private readonly Point _target;

  public POrderMoveTo(Pac owner, Point target) : base(owner)
  {
    _target = target;
  }

  public override bool IsCompleted(Context cx)
  {
    return Owner.Pos == _target;
  }

  public override bool Execute(Context cx)
  {
    Owner.Move(_target);
    return true;
  }
}