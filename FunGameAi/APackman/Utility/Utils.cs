using System;
using System.Collections.Generic;
using System.Linq;

public static class Utils
{
  public static bool CHasFlag(this CellFlags flags, CellFlags f)
  {
    return f >= 0 && (flags & f) == f || f < 0 && (flags & f) == flags;
  }

  /*public static List<T> ToList<T>(this T val)
  {
    return new List<T>();
  }*/

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

  public static T FindMax<T, TValue>(this List<T> list, Func<T, TValue> predicate)
    where TValue : IComparable<TValue>
  {
    T result = list[0];
    var bestMax = predicate(result);
    for (int i = 1; i < list.Count; i++)
    {
      var item = list[i];
      var v = predicate(item);
      if (v.CompareTo(bestMax) > 0)
      {
        bestMax = v;
        result = item;
      }
    }

    return result;
  }

  public static unsafe ushort[] Add(this ushort[] m1, ushort[] m2)
  {
    int l = m1.Length;
    unsafe
    {
      fixed (ushort*  pm1 = m1, pm2 = m2)
      {
        for (int i = 0; i < l; i++)
        {
          pm1[i] += pm2[i];
        }
      }
    }

    return m1;
  }

  public static unsafe float[,] Add(this float[,] m1, float[,] m2)
  {
    int l = m1.Length;
    var m3 = new float[m1.GetLength(0), m1.GetLength(1)];
    unsafe
    {
      fixed (float* pm1 = m1, pm2 = m2, pm3 = m3)
      {
        for (int i = 0; i < l; i++)
        {
          pm3[i] = pm1[i] + pm2[i];
        }
      }
    }

    return m3;
  }

  public static unsafe float[,] Subtract(this float[,] m1, float[,] m2)
  {
    int l = m1.Length;
    var m3 = new float[m1.GetLength(0), m1.GetLength(1)];
    unsafe
    {
      fixed (float* pm1 = m1, pm2 = m2, pm3 = m3)
      {
        for (int i = 0; i < l; i++)
        {
          pm3[i] = pm1[i] - pm2[i];
        }
      }
    }

    return m3;
  }
}