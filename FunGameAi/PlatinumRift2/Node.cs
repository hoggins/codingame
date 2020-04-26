class Node : GridNode
{
  public int OwnerId;
  public int MyPods;
  public int EnemyPods;
  public bool Visible;
  public int Platinum;
  public override int Value => Platinum;
}