using System.Linq;

public class BehTree
{
  public static void Test(Context cx, Pac pac)
  {
    var enemy = cx.Pacs.Where(p => !p.IsMine).Select(p => (p: p, dist: p.Pos.Distance(pac.Pos)))
      .FindMin(p => p.dist);


    // def
    if (enemy.p != null && enemy.dist <= 1 && !pac.CanBeat(enemy.p) && pac.CanUseAbility)
      SwitchToCounter(cx, pac, enemy);
    // attack
    else if (enemy.p != null && enemy.dist <= 2 && pac.CanBeat(enemy.p))
      Attack(cx, pac, enemy);
    // seek
    else if (pac.VisiblePellets >= 14 && !pac.IsBoosted && pac.CanUseAbility)
      Boost(cx, pac, enemy);
    else
      ProceedOrSeek(cx, pac, enemy);

    // todo flee?
  }

  private static void Attack(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    pac.Order = new POrderMoveToEnemy(pac, enemy.p.Pos);
  }

  private static void Boost(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    pac.Order = new POrderBoost(pac);
  }

  private static void SwitchToCounter(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    pac.Order = new POrderSwitch(pac, Rules.GetVulnerability(enemy.p.Type));
  }

  private static void ProceedOrSeek(Context cx, Pac pac, (Pac p, int dist) enemy)
  {
    if (pac.Order == null || pac.Order.IsCompleted(cx))
    {
      pac.Order = null;
      var pellet = cx.Map.FindNearest(pac.Pos, CellFlags.Pellet)
                   //?? cx.Map.FindNearest(pac.Pos, CellFlags.HadPellet)
                   ?? cx.Map.FindNearest(pac.Pos, ~CellFlags.Seen);
      if (pellet.HasValue)
        pac.Order = new POrderMoveTo(pac, pellet.Value);
      else
      {
        Player.Print($"no pellet for {pac}");
      }
    }
  }
}