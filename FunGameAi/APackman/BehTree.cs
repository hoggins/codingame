using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Prediction
{
  public Pac Pac;
  public List<List<Path>> Path = new List<List<Path>>();
  // pac path[0] * -> 1 pathFromP[0]
}

public class BehTree
{
  private List<Pac> _enemyPacs;
  private readonly List<Point> _allocatedPellets = new List<Point>();
  private IOrderedEnumerable<(Cell pellets, Pac pac, Path path)> _allPathCache;

  private List<Pac> _queuedForPath = new List<Pac>();

  public void UpdateTick(Context cx)
  {
    UpdateEnemyPacPos(cx);

    foreach (var pac in cx.Pacs.Where(p => p.IsMine))
    {
      if (pac.Order?.IsCompleted(cx) == true)
        pac.SetOrder(cx,null);
    }

    TryHuntGems(cx);
  }

  private void UpdateEnemyPacPos(Context cx)
  {
    if (cx.Tick == 1)
    {
      _enemyPacs = new List<Pac>();
      var rowLen = cx.Field.Width;
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
    return;
    if (cx.Field.Gems.Count == 0)
      return;
    var cells = Enumerable.ToList(cx.Field.Gems.Where(p => !_allocatedPellets.Contains(p)).Select(p => cx.Field[p.Y, p.X]));
    if (cells.Count == 0)
      return;
    var pacs = cx.Pacs.Where(p => p.IsMine && p.Order == null)
      .Union(_enemyPacs)
      ;
    var pathFromAllPacs = cx.Tick == 2
        ? _allPathCache
        : pacs
          .SelectMany(p => cells.Select(l => (pellets: l, pac: p, path: cx.Field.FindPath(p.Pos, l.Pos))))
          .Where(p => p.path != null)
          .OrderBy(p => p.path.Count)
      ;

    if (cx.Tick == 1)
    {
      _allPathCache = pathFromAllPacs;
      return;
    }

    /*foreach (var (pellets, pac, path) in pathFromAllPacs
      .OrderBy(p=>((int)p.pellets.Pos.X <<16) + p.pellets.Pos.X)
      .ThenBy(p => p.path.Count))
      Player.Print($"P:{pellets} C:{pac} LEN:{path.Count}");*/

    foreach (var (pellets, pac, path) in pathFromAllPacs)
    {
      if (_allocatedPellets.Contains(pellets.Pos))
        continue;
      _allocatedPellets.Add(pellets.Pos);

      if (pac.Order != null)
        pac.Order.OnCompleted += () => _allocatedPellets.Remove(pellets.Pos);
      else if (pac.IsMine)
      {
        var order = new POrderMoveToPellet(pac, cx.Field, pellets.Pos);
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


    // flee
    if (enemy.p != null && enemy.dist <= dangerRadius && enemy.p.CanBeat(pac) && !IsInAdvance(cx, pac, enemy.p, attackRadius) && CanFlee(cx, pac, enemy.p, attackRadius))
      Flee(cx, pac, enemy.p, attackRadius);

    // def
    else if (enemy.p != null && enemy.dist <= dangerRadius && enemy.p.CanBeat(pac) && pac.CanUseAbility)
      SwitchToCounter(cx, pac, enemy);

    // attack
    else if (enemy.p != null && enemy.dist <= attackRadius && pac.CanBeat(enemy.p) && enemy.p.AbilityCooldown > 2 && !CanEscape(cx, pac, enemy))
      Attack(cx, pac, enemy);

    // be smart
    else if (pac.IsInClutch && pac.CanUseAbility)
      pac.SetOrder(cx, null); // skip turn

    // seek
    // todo use boost only on (odd, odd) coords: all crosroads on !odd coordinates
    else if ( /*pac.VisiblePellets >= 14 &&*/ !pac.IsBoosted && pac.CanUseAbility)
      Boost(cx, pac, enemy);
    else
      ProceedOrSeek(cx, pac, enemy);

    // todo flee?
  }

  private bool IsInAdvance(Context cx, Pac pac, Pac enemy, int attackRadius)
  {
    if (!(pac.Order is POrderMoveByPath order))
      return false;

    var path = order.GetTurnPath();
    return path.Last().Distance(enemy.Pos) > attackRadius;
  }

  private void Flee(Context cx, Pac pac, Pac enemy, int attackRadius)
  {
    var fleePoint = GetFleePoint(cx, pac, enemy, attackRadius, true)
                    ?? GetFleePoint(cx, pac, enemy, attackRadius, false);
    pac.SetOrder(cx, new POrderMoveToPellet(pac, cx.Field, fleePoint.Value));
  }

  private bool CanFlee(Context cx, Pac pac, Pac enemy, int attackRadius)
  {
    return (GetFleePoint(cx, pac, enemy, attackRadius, true)
           ?? GetFleePoint(cx, pac, enemy, attackRadius, false))
           != null;
  }

  private Point? GetFleePoint(Context cx, Pac pac, Pac enemy, int range, bool withPelet)
  {
    var src = pac.Pos;
    var field = cx.Field;
    int colLen = field.Height;
    int rowLen = field.Width;
    for (int j = 0; j < 4; j++)
    {
      var adj = new Point(src.X + AStarUtil.ColNum[j], src.Y + AStarUtil.RowNum[j]);
      AStarUtil.Warp(ref adj, rowLen, colLen);
      if (!AStarUtil.IsValid(adj, rowLen, colLen)) continue;
      if (!field.CanTraverse(adj)) continue;
      if (adj == enemy.Pos) continue;
      if (adj.Distance(enemy.Pos) <= range) continue;
      if (withPelet && field.GetFlags(adj).CHasFlag(CellFlags.Pellet))
        return adj;
      if (!withPelet)
        return adj;
    }

    return null;
  }

  private bool CanEscape(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    var src = enemy.p.Pos;
    var field = cx.Field;
    int colLen = field.Height;
    int rowLen = field.Width;
    for (int j = 0; j < 4; j++)
    {
      var adj = new Point(src.X + AStarUtil.ColNum[j], src.Y + AStarUtil.RowNum[j]);
      AStarUtil.Warp(ref adj, rowLen, colLen);
      if (!AStarUtil.IsValid(adj, rowLen, colLen)) continue;
      if (!field.CanTraverse(adj)) continue;
      if (adj == pac.Pos) continue;
      return true;
    }

    return false;
  }

  private static void Attack(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    pac.SetOrder(cx, new POrderMoveToEnemy(pac, cx.Field, enemy.p.Pos));
  }

  private static void Boost(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    pac.SetOrder(cx, new POrderBoost(pac));
  }

  private static void SwitchToCounter(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    pac.SetOrder(cx, new POrderSwitch(pac, Rules.GetVulnerability(enemy.p.Type)));
  }

  private void ProceedOrSeek(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    if (pac.Order != null)
      return;
    //pac.SetOrder(cx, null);

    _queuedForPath.Add(pac);
  }

  public void UpdatePostTick(Context cx)
  {
    ProcessPathQueue(cx);
  }

  private void ProcessPathQueue(Context cx)
  {
    if (_queuedForPath.Count == 0)
      return;

    var sim = new Simulator(cx.Field.Height, cx.Field.Width);
    var orders = sim.RunBest(cx.Field, _queuedForPath);
    foreach (var pair in orders)
    {
      // Player.Print($"goal {pair.Key} : {pair.Value}");
      var pac = pair.Key;
      // if (pac.Order is POrderMoveByBestPath pOrder)
      // {
      //   var curValue = CalcPathValue(cx, pOrder._path);
      //   var newValue = CalcPathValue(cx, pair.Value);
      //   if (curValue + 1 > newValue)
      //     continue;
      // }
      pac.SetOrder(cx, new POrderMoveByBestPath(pac, pair.Value));
    }
    _queuedForPath.Clear();
  }

  private float CalcPathValue(Context cx, Path path)
  {
    return path.Sum(p => Balance.GetCellValue(cx.Field.GetFlags(p)));
  }

  private void ProcessPathQueueDouble(Context cx)
  {
    var cost = new Map<float>(cx.Field.Height, cx.Field.Width);
    foreach (var allocatedPellet in _allocatedPellets)
    {
      cx.Infl.CostMap[allocatedPellet] += 3;
      cost[allocatedPellet] += 3;
    }

    var preds = new List<Prediction>();

    Predictions = preds;

    foreach (var pac in _queuedForPath.OrderByDescending(p=>cx.Infl.CostMap[p.Pos]))
    {
      var pathOptions = cx.Field.FindMultyPath(pac.Pos, 6, 11, cx.Infl.CostMap);
      if (pathOptions.Count == 0)
      {
        if (!TrySeek(cx, pac))
          Player.Print("no path");
      }
      else
      {
        var prediction = new Prediction{Pac = pac};
        foreach (var option in pathOptions)
        {
          prediction.Path.Add(new List<Path>{option});
        }

        preds.Add(prediction);
      }
    }

    // foreach (var p in preds.SelectMany(p=>p.Path[0]).SelectMany(p=>p).Distinct())
        // ++cx.Infl.CostMap[p];

        foreach (var p in preds)
        foreach (var path in p.Path)
        {
          cx.Infl.PlacePac(cx, path.Last().Last());
        }

    foreach (var pred in preds)
    {
      var pac = pred.Pac;
      // var nextPaths = new List<Path>();
      // pred.Path.Add(nextPaths);
      foreach (var bundle in pred.Path)
      {
        var bestPath = cx.Field.FindBestPath(bundle[0].Last(), 6, 8, cx.Infl.CostMap);
        bundle.Add(bestPath);
      }
    }

    // todo define leading pac
    for (var i = 0; i < preds.Count; i++)
    {
      var pred = preds[i];


      var best = pred.Path.FindMax(g => g.Sum(p => p.Value) / g.Sum(p=>p.Count));
      var path = new Path(best[0]);
      path.AddRange(best[1]);
      pred.Pac.SetOrder(cx, new POrderMoveByBestPath(pred.Pac, path));

      // todo update wights of paths for other pacs
    }

    /*foreach (var pac in _queuedForPath.OrderByDescending(p=>cx.Infl.CostMap[p.Pos]))
    {
      Player.Print("THE THING : " + pac);
      var bestPath = cx.Field.FindBestPath(pac.Pos, 6, 12, cx.Infl.CostMap);
      if (bestPath != null && !bestPath.IsZero)
      {
        pac.SetOrder(cx, new POrderMoveByBestPath(pac, bestPath));
        foreach (var p in bestPath)
          ++cx.Infl.CostMap[p];
      }
      else
      {
        if (!TrySeek(cx, pac))
          Player.Print("no path");
      }
    }*/

    _queuedForPath.Clear();
  }

  public List<Prediction> Predictions { get; set; }

  private static bool TrySeek(Context cx, Pac pac)
  {
    {
      var nPoint = cx.Field.FindNearest(pac.Pos, ~CellFlags.Seen);
      if (nPoint.HasValue)
      {
        pac.SetOrder(cx, new POrderMoveToDiscovery(pac, cx.Field, nPoint.Value));
        Player.Print($"found point s {nPoint.Value} flags {cx.Field.GetFlags(nPoint.Value)}");
        return true;
      }
    }
    {
      var nPoint = cx.Field.FindNearest(pac.Pos, CellFlags.HadPellet);
      if (nPoint.HasValue)
      {
        pac.SetOrder(cx, new POrderMoveToPellet(pac, cx.Field, nPoint.Value));
        Player.Print($"found point h {nPoint.Value} flags {cx.Field.GetFlags(nPoint.Value)}");
        return true;
      }
    }


    return false;

  }
}