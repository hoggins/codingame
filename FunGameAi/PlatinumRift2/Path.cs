using System;
using System.Collections.Generic;

class Path : List<int>
{
  public int Depth;

  public Path()
  {
  }

  public Path(int capacity) : base(capacity)
  {
  }

  public Path(IEnumerable<int> collection) : base(collection)
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

  public int NextNodeId(int fromNode)
  {
    var curIdx = FindIndex(fromNode);
    if (curIdx == Count - 1)
      throw new Exception($"{fromNode} not on path {this}");

    var next = this[curIdx + 1];
    return next;
  }
}