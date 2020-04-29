using System.Linq;

class SOrderSeekForEnemy : SOrderBase
{
  public SOrderSeekForEnemy(Squad owner) : base(owner)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return false;
  }

  public override bool Execute(Context cx)
  {
    // we are fighting
    var curNode = cx.Nodes[Owner.NodeId];
    if (curNode.EnemyPods > 0)
      return false;

    var bestNode = curNode.DistToEnemyBase <= 5
      ? GetBestNode(cx, Owner)
      : GetDefPoint(cx, Owner)
        ?? SOrderSeekForPlatinum.GetBestNode(cx, Owner, out _)
        ?? GetBestNode(cx, Owner);
    if (bestNode == null)
      return false;

    // todo split large squads and send half on neighbor cell when needed

    cx.MoveTo(bestNode.Id, Owner);
    return true;
  }

  private static Node GetBestNode(Context cx, Squad owner)
  {
    var curNode = cx.Nodes[owner.NodeId];
    var originNode = owner.LastVisited.LastOrDefault();
    var bestNode = curNode.Connections
      .Where(n => originNode != n)
      .Select(i => cx.Nodes[i])
      .FindMin(n =>
        /*n.EnemyPods > 0 && n.Incomming.Count < curNode.MyPods / 2 ? n.DistToEnemyBase - 1 :*/ n.DistToEnemyBase);
    return bestNode;
  }

  private static Node GetDefPoint(Context cx, Squad owner)
  {
    var curNode = cx.Nodes[owner.NodeId];
    var originNode = owner.LastVisited.LastOrDefault();
    var bestNode = curNode.Connections
      .Where(n => originNode != n)
      .Select(i => cx.Nodes[i])
      .Where(n=>n.EnemyPods > n.Incomming.Count)
      .FindMin(n => n.DistToEnemyBase);
    return bestNode;
  }
}