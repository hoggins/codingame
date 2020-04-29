using System.Collections.Generic;

class Node
{
  public int Id;
  public int[] Connections = new int[0];

  public int DistToMyBase;
  public int DistToEnemyBase;

  public int OwnerId;
  public int MyPods;
  public int EnemyPods;
  public bool Visible;
  public int Platinum;

  public int? PlatinumMax;
  public int? LastOwner;
  public List<int> Incomming = new List<int>();

  public bool IsMine;
}