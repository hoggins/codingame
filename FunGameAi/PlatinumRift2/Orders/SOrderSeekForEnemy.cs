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
    if (cx.Nodes[Owner.NodeId].EnemyPods > 0)
      return false;

    var curNode = cx.Nodes[Owner.NodeId];
    var originNode = Owner.LastVisited.LastOrDefault();
    var bestNode = curNode.Connections
      .Where(n => originNode != n)
      .Select(i => cx.Nodes[i])
      .FindMin(n => n.EnemyPods > 0 && n.Incomming.Count < curNode.MyPods / 2 ? n.DistToEnemyBase - 1 : n.DistToEnemyBase);

    if (bestNode == null)
      return false;

    // todo split large squads and send half on neighbor cell when needed

    cx.MoveTo(bestNode.Id, Owner);
    return true;
  }
}