using System;
using System.Collections.Generic;
using System.Linq;

public static class Utils
{
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