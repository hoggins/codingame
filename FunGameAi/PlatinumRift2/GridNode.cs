abstract class GridNode
{
  public int Id;
  public int[] Connections = new int[0];
  public abstract int Value { get; }
}