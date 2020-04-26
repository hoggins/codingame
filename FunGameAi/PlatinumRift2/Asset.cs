using System;

class Asset
{
  public int NodeId;
  public int Pods;

  public int? NextTurnNodeId;

  public Asset(int nodeId, int pods)
  {
    NodeId = nodeId;
    Pods = pods;
  }

  public void MoveTo(int nodeId)
  {
    if (NextTurnNodeId.HasValue)
      throw new Exception("already have order");
    NextTurnNodeId = nodeId;
    Console.Write($"{Pods} {NodeId} {nodeId} ");
  }

  public void CommitMovement()
  {
    if (!NextTurnNodeId.HasValue)
      return;
    NodeId = NextTurnNodeId.Value;
    NextTurnNodeId = null;
  }
}