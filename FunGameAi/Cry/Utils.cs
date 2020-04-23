using System;
using System.Collections.Generic;
using System.Linq;

public static class Utils
{
  public static IEnumerable<(int, int)> EnumerateNeighbors((int, int) from, int shift = 1)
  {
    var (x, y) = @from;
    if (x + shift < 30)
      yield return (x + shift, y);
    //yield return (x, y);
    if (y+shift < 15)
      yield return (x, y+shift);
    if (y-shift > 0)
      yield return (x, y-shift);
    yield return (x-shift, y);
  }

  public static int Distance((int, int) p1, (int, int) p2)
  {
    return Math.Abs(p1.Item1 - p2.Item1) + Math.Abs(p1.Item2 - p2.Item2);
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