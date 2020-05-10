using System;
using System.Collections.Generic;

public class Path : List<Point>
{
  public int Value;

  public Path()
  {
  }

  public Path(int capacity) : base(capacity)
  {
  }

  public Path(IEnumerable<Point> collection) : base(collection)
  {
  }

  public int FindIndex(Point targetId)
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

  public Point NextPoint(Point fromNode)
  {
    var curIdx = FindIndex(fromNode);
    if (curIdx == -1)
      throw new Exception($"{fromNode} not on path {this}");

    var next = this[curIdx + 1];
    return next;
  }
}