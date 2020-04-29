using System.Collections.Generic;
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
      .Count(n => !cx.IsMe(n.OwnerId) && n.Incomming.Count == 0 /*&& Owner.LastVisited.LastOrDefault() != n.Id*/);
    return adjNodes == 0;
  }

  public override bool Execute(Context cx)
  {
    // we are fighting
    if (cx.Nodes[Owner.NodeId].EnemyPods > 0)
      return false;

    var curNode = cx.Nodes[Owner.NodeId];
    var best = GetBestNode(cx, Owner, out var adjNodes);
    if (best == null)
      best = adjNodes[cx.Random.Next(adjNodes.Count)];

    if (best.EnemyPods > 0 && best.EnemyPods <= Owner.Pods && best.Platinum <= curNode.Platinum)
      return false;

    cx.MoveTo(best.Id, Owner);
    return true;
  }

  public static Node GetBestNode(Context cx, Squad owner, out List<Node> adjNodes)
  {
    var curNode = cx.Nodes[owner.NodeId];
    adjNodes = curNode.Connections
      .Select(i => cx.Nodes[i])
      .Where(n => !cx.IsMe(n.OwnerId) && n.Incomming.Count == 0 /*&& owner.LastVisited.LastOrDefault() != n.Id*/)
      .ToList();
    var best = adjNodes.FindMax(n => n.Platinum);
    return best;
  }
}