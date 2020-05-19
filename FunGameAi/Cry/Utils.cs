using System;
using System.Collections.Generic;
using System.Linq;

public static class Utils
{
  public static IEnumerable<Point> EnumerateNeighbors(Point from, int shift = 1)
  {
    var (x, y) = @from;
    if (x + shift < 30)
      yield return new Point(x + shift, y);
    //yield return (x, y);
    if (y+shift < 15)
      yield return new Point(x, y+shift);
    if (y-shift > 0)
      yield return new Point(x, y-shift);
    yield return new Point(x-shift, y);
  }

  public static int Distance(Point p1, Point p2)
  {
    return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
  }

  public static T FindMin<T, TValue>(this IEnumerable<T> list, Func<T, TValue> predicate)
    where TValue : IComparable<TValue>
  {
    T result = list.FirstOrDefault();
    if (result != null)
    {
      var bestMin = predicate(result);
      foreach (var item in list.Skip(1))
      {
        var v = predicate(item);
        if (v.CompareTo(bestMin) < 0)
        {
          bestMin = v;
          result = item;
        }
      }
    }

    return result;
  }

  public static T FindMax<T, TValue>(this IEnumerable<T> list, Func<T, TValue> predicate)
    where TValue : IComparable<TValue>
  {
    T result = list.FirstOrDefault();
    if (result != null)
    {
      var bestMax = predicate(result);
      foreach (var item in list.Skip(1))
      {
        var v = predicate(item);
        if (v.CompareTo(bestMax) > 0)
        {
          bestMax = v;
          result = item;
        }
      }
    }

    return result;
  }
}