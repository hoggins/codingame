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



    var bestNode = GetBestNode(cx, curNode);
    if (bestNode == null)
      return false;

    // todo split large squads and send half on neighbor cell when needed

    cx.MoveTo(bestNode.Id, Owner);
    return true;
  }

  private Node GetBestNode(Context cx, Node curNode)
  {
    var nodes = curNode.Connections.Select(i => cx.Nodes[i]).ToArray();
    var hasBackup = Owner.LastVisited.Count > 0 && cx.Nodes[Owner.LastVisited.Last()].MyPods > 0;
    if (!hasBackup || curNode.DistToEnemyBase <= 5)
      return GetBestAttack(cx, Owner, curNode, nodes);

    var scout = GetCapturePlatinumOrRecapture(cx, Owner, curNode, nodes);
    if (scout != null)
      return scout;

    var def = GetDefPoint(cx, Owner, curNode, nodes);
    if (def != null)
      return def;

    var att = GetBestAttack(cx, Owner, curNode, nodes);

    if (att.EnemyPods > 0)
      return null;

    return att;
  }

  private static Node GetBestAttack(Context cx, Squad owner, Node curNode, Node[] nodes)
  {
    var originNode = owner.LastVisited.LastOrDefault();
    var bestNode = nodes
      .FindMin(n => originNode != n.Id ? n.DistToEnemyBase : n.DistToEnemyBase + 1);
    return bestNode;
  }

  private static Node GetCapturePlatinumOrRecapture(Context cx, Squad owner, Node curNode, Node[] nodes)
  {
    var bestNode = nodes.Where(n=>(n.Platinum > 0 && !cx.IsMe(n.OwnerId)) || cx.IsEnemy(n.OwnerId))
      .FindMin(n => n.DistToEnemyBase);
    return bestNode;
  }

  private static Node GetDefPoint(Context cx, Squad owner, Node curNode, Node[] nodes)
  {
    var originNode = owner.LastVisited.LastOrDefault();
    var bestNode = nodes
      .Where(n=>n.EnemyPods > n.Incomming.Count)
      .FindMin(n => originNode != n.Id ? n.DistToEnemyBase : n.DistToEnemyBase + 1);
    return bestNode;
  }
}