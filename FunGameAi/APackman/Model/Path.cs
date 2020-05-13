using System;
using System.Collections.Generic;
using System.Linq;

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

  public Point NextPoint(Point fromNode, int speed = 1)
  {
    var curIdx = FindIndex(fromNode);
    if (curIdx == -1)
      throw new Exception($"{fromNode} not on path {this}");

    var nIdx = Math.Min(Count - 1, curIdx + speed);

    var next = this[nIdx];
    return next;
  }

  public List<Point> NextPath(Point fromNode, int speed = 1)
  {
    var curIdx = FindIndex(fromNode);
    if (curIdx == -1)
      throw new Exception($"{fromNode} not on path {this}");

    var nIdx = curIdx+1;//Math.Min(Count-1, curIdx);

    return this.Skip(nIdx).Take(speed).ToList();
  }
}