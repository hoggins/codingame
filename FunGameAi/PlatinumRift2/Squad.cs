using System;
using System.Collections.Generic;

class Squad
{
  public int Id;
  public SOrderBase Order;
  public int NodeId;
  public int Pods;

  public int? NextTurnNodeId;

  public Squad(int nodeId, int pods)
  {
    NodeId = nodeId;
    Pods = pods;
  }


}