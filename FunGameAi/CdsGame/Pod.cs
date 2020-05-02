using System.Numerics;

class Pod
{
  public bool IsMine;
  public Vector2 Pos;
  public Vector2 Velocity;
  public int Rotation;
  public int NextNodeId;
  public bool IsBoostUsed;

  public Node NextNode(Context cx) => cx.Nodes[NextNodeId];
}