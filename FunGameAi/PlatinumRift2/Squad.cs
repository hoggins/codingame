class Squad
{
  public readonly int Id;
  public SOrderBase Order;
  public int NodeId;
  public int Pods;

  public Squad(int id, int nodeId, int pods)
  {
    Id = id;
    NodeId = nodeId;
    Pods = pods;
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(null, obj))
      return false;
    if (obj is Squad other)
      return Id == other.Id;
    return false;
  }

  public override int GetHashCode()
  {
    return Id.GetHashCode();
  }
}