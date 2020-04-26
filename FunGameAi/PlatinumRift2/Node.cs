using System.Collections.Generic;

class Node : GridNode
{
  public int OwnerId;
  public int MyPods;
  public int EnemyPods;
  public bool Visible;
  public int Platinum;
  public int? PlatinumMax;
  public int? LastOwner;
  public List<int> Incomming = new List<int>();
  public override int Value => Platinum;

  public bool IsMine;
}