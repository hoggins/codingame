using System;
using System.Collections.Generic;
using System.Linq;

public class BehTree
{

  public void UpdateTick(Context cx)
  {
    foreach (var pac in cx.Pacs.Where(p => p.IsMine))
    {
      if (pac.Order?.IsCompleted(cx) == true)
        pac.SetOrder(cx,null);
    }

    if (cx.Map.Gems.Count > 0)
    {
      var cells = cx.Map.Gems.Select(p => cx.Map.Grid[p.Y, p.X]).ToList();
      var weights = cx.Pacs.Where(p => p.IsMine && p.Order == null)
        .SelectMany(p=>cells.Select(l=>(pellets:l, pac:p, path:cx.Map.FindPath(p.Pos, l.Pos))))
        .Where(p=>p.path != null)
        .OrderBy(p=>p.path.Count)
        .ToList();

      var allocatedPellets = new List<Point>();
      foreach (var w in weights)
      {
        if (allocatedPellets.Contains(w.pellets.Pos)
            || w.pac.Order != null)
          continue;
        allocatedPellets.Add(w.pellets.Pos);
        w.pac.SetOrder(cx, new POrderMoveToPellet(w.pac, w.pellets.Pos));
      }
    }
  }

  public void UpdateOrder(Context cx, Pac pac)
  {
    var enemy = cx.Pacs.Where(p => !p.IsMine).Select(p => (p: p, dist: p.Pos.Distance(pac.Pos)))
      .FindMin(p => p.dist);

    var dangerRadius = 1 + (enemy.p?.IsBoosted == true ? 1 : 0);
    var attackRadius = 1 + (pac.IsBoosted ? 1 : 0);

    // def
    if (enemy.p != null && enemy.dist <= dangerRadius && !pac.CanBeat(enemy.p) && pac.CanUseAbility)
      SwitchToCounter(cx, pac, enemy);
    // attack
    else if (enemy.p != null && enemy.dist <= attackRadius && pac.CanBeat(enemy.p))
      Attack(cx, pac, enemy);
    // seek
    else if (/*pac.VisiblePellets >= 14 &&*/ !pac.IsBoosted && pac.CanUseAbility)
      Boost(cx, pac, enemy);
    else
      ProceedOrSeek(cx, pac, enemy);

    // todo flee?
  }

  private static void Attack(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    pac.SetOrder(cx, new POrderMoveToEnemy(pac, enemy.p.Pos));
  }

  private static void Boost(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    pac.SetOrder(cx, new POrderBoost(pac));
  }

  private static void SwitchToCounter(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    pac.SetOrder(cx, new POrderSwitch(pac, Rules.GetVulnerability(enemy.p.Type)));
  }

  private static void ProceedOrSeek(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    if (pac.Order != null)
      return;
    pac.SetOrder(cx, null);
    var options = new[]
    {
      cx.Map.FindNearest(pac.Pos, CellFlags.Pellet,2),
      cx.Map.FindNearest(pac.Pos, CellFlags.HadPellet, 2),
      cx.Map.FindNearest(pac.Pos, ~CellFlags.Seen, 2)
    };

    // Player.Print("for owner " + pac);
    // foreach (var option in options)
    // {
      // Player.Print("p: " + option);
    // }

    var pellet = options.Where(p => p.HasValue).FindMin(p => p.Value.Distance(pac.Pos));

    if (pellet.HasValue)
      pac.SetOrder(cx, new POrderMoveToPellet(pac, pellet.Value));
    else
    {
      Player.Print($"no pellet for {pac}");
    }
  }
}