using System.Linq;

class SOrderSeekForPlatinum : SOrderBase
{
  public SOrderSeekForPlatinum(Squad owner) : base(owner)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    var curNode = cx.Nodes[Owner.NodeId];
    var adjNodes = curNode.Connections.Select(i => cx.Nodes[i])
      .Count(n => !cx.IsMe(n.OwnerId) && n.Incomming.Count == 0);
    return adjNodes == 0;
  }

  public override bool Execute(Context cx)
  {
    var curNode = cx.Nodes[Owner.NodeId];
    var adjNodes = curNode.Connections.Select(i => cx.Nodes[i])
      .Where(n => !cx.IsMe(n.OwnerId) && n.Incomming.Count == 0).ToList();
    var best = adjNodes.FindMax(n => n.Platinum);
    if (best == null)
      best = adjNodes[cx.Random.Next(adjNodes.Count)];

    cx.MoveTo(best.Id, Owner);
    return true;
  }
}

class SOrderSeekForEnemy : SOrderBase
{
  public SOrderSeekForEnemy(Squad owner) : base(owner)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return false;
    // var curNode = cx.Nodes[Owner.NodeId];
    // var adjNodes = curNode.Connections.Select(i => cx.Nodes[i]).Count(n => !cx.IsMe(n.OwnerId) && n.Incomming.Count == 0);
    // return adjNodes == 0;
  }

  public override bool Execute(Context cx)
  {
    var curNode = cx.Nodes[Owner.NodeId];
    var originNode = Owner.LastVisited.LastOrDefault();
    var bestNode = curNode.Connections
      .Where(n => originNode != n)
      .Select(i => cx.Nodes[i])
      .FindMin(n => n.EnemyPods > 0 ? n.DistToEnemyBase - 1 : n.DistToEnemyBase);



    cx.MoveTo(bestNode.Id, Owner);
    return true;
  }
}