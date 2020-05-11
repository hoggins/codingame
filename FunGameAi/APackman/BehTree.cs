using System;
using System.Collections.Generic;
using System.Linq;

public class BehTree
{
  private List<Pac> _enemyPacs;
  private List<Point> _allocatedPellets = new List<Point>();

  public void UpdateTick(Context cx)
  {
    UpdateEnemyPacPos(cx);

    foreach (var pac in cx.Pacs.Where(p => p.IsMine))
    {
      if (pac.Order?.IsCompleted(cx) == true)
        pac.SetOrder(cx,null);
    }

    if (cx.Tick > 1)
      TryHuntGems(cx);
  }

  private void UpdateEnemyPacPos(Context cx)
  {
    if (cx.Tick == 1)
    {
      _enemyPacs = new List<Pac>();
      var rowLen = cx.Map.Grid.GetLength(1);
      var halfField = rowLen / 2;
      foreach (var pac in cx.Pacs.Where(p => p.IsMine))
      {
        var x = pac.Pos.X;
        var shift = x > halfField ? (x - halfField) * -1 : halfField - x;
        _enemyPacs.Add(new Pac(-1) {Pos = new Point(halfField + shift, pac.Pos.Y)});
      }
    }
  }

  private void TryHuntGems(Context cx)
  {
    if (cx.Map.Gems.Count == 0)
      return;
    var cells = cx.Map.Gems.Where(p => !_allocatedPellets.Contains(p)).Select(p => cx.Map.Grid[p.Y, p.X]).ToList();
    if (cells.Count == 0)
      return;
    var pacs = cx.Pacs.Where(p => p.IsMine && p.Order == null)
      .Union(_enemyPacs)
      ;
    var pathFromAllPacs = pacs
        .SelectMany(p => cells.Select(l => (pellets: l, pac: p, path: cx.Map.FindPath(p.Pos, l.Pos))))
        .Where(p => p.path != null)
        .OrderBy(p => p.path.Count)
      ;

    /*foreach (var (pellets, pac, path) in pathFromAllPacs
      .OrderBy(p=>((int)p.pellets.Pos.X <<16) + p.pellets.Pos.X)
      .ThenBy(p => p.path.Count))
    {
      Player.Print($"P:{pellets} C:{pac} LEN:{path.Count}");
    }*/

    foreach (var (pellets, pac, path) in pathFromAllPacs)
    {
      if (_allocatedPellets.Contains(pellets.Pos))
        continue;
      _allocatedPellets.Add(pellets.Pos);

      if (pac.Order != null)
        pac.Order.OnCompleted += () => _allocatedPellets.Remove(pellets.Pos);
      else if (pac.IsMine)
      {
        var order = new POrderMoveToPellet(pac, pellets.Pos);
        order.OnCompleted += () => _allocatedPellets.Remove(pellets.Pos);
        pac.SetOrder(cx, order);
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
    // todo can escape && can counter conditions; otherwise attack is waste of time or pac
    else if (enemy.p != null && enemy.dist <= attackRadius && pac.CanBeat(enemy.p) && enemy.p.AbilityCooldown > 2)
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
    if (pac.Order != null && !(pac.Order is POrderMoveByPath))
      return;
    pac.SetOrder(cx, null);

    // Player.Print("LOOKING : " + pac);
    var bestPath = cx.Map.FindBestPath(pac.Pos, 8, 10);
    if (bestPath != null)
      pac.SetOrder(cx, new POrderMoveByPath(pac, bestPath));
    else
    {
      if (!TrySeek(cx, pac))
        Player.Print("no path");

    }
    /*var options = new[]
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
    }*/


  }

  private static bool TrySeek(Context cx, Pac pac)
  {
    {
      var nPoint = cx.Map.FindNearest(pac.Pos, ~CellFlags.Seen);
      if (nPoint.HasValue)
      {
        pac.SetOrder(cx, new POrderMoveToDiscovery(pac, nPoint.Value));
        Player.Print($"found point s {nPoint.Value} flags {cx.Map.GetFlags(nPoint.Value)}");
        return true;
      }
    }
    {
      var nPoint = cx.Map.FindNearest(pac.Pos, CellFlags.HadPellet);
      if (nPoint.HasValue)
      {
        pac.SetOrder(cx, new POrderMoveToPellet(pac, nPoint.Value));
        Player.Print($"found point h {nPoint.Value} flags {cx.Map.GetFlags(nPoint.Value)}");
        return true;
      }
    }


    return false;

  }
}