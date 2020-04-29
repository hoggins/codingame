using System.Collections.Generic;

class Path : List<int>
{
  public Path()
  {
  }

  public Path(Path other)
    :base (other as List<int>)
  {
  }

  public int FindIndex(int targetId)
  {
    for (var i = 0; i < Count; i++)
    {
      var nodeId = this[i];
      if (nodeId == targetId)
        return i;
    }

    return -1;
  }

  public override string ToString()
  {
    return string.Join(" ", this);
  }
}