using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Don't let the machines win. You are humanity's last hope...
 **/
class PlayerArray
{
  static void MainArray(string[] args)
  {
    var none = "-1 -1";

    int width = int.Parse(Console.ReadLine()); // the number of cells on the X axis
    int height = int.Parse(Console.ReadLine()); // the number of cells on the Y axis
    var m = new bool[height, width];
    for (int i = 0; i < height; i++)
    {
      var l = Console.ReadLine();
      for (int j = 0; j < l.Length; j++)
      {
        m[i, j] = l[j] == '0';
      }
    }

    // int width = 2;
    // int height = 2;
    // var m = new bool[,]{{true, true},{true,false}};

    // Write an action using Console.WriteLine()
    // To debug: Console.Error.WriteLine("Debug messages...");

    for (int i = 0; i < height; i++)
    for (int j = 0; j < width; j++)
    {
      if (!m[i,j])
        continue;
      var o = new List<string>();
      o.Add($"{j} {i}");
      {
        var n = FindNext(m, i, j, 0, 1);
        o.Add(!n.HasValue ? none : $"{n.Value.Item2} {n.Value.Item1}");
      }
      {
        var n = FindNext(m, i, j, 1, 0);
        o.Add(!n.HasValue ? none : $"{n.Value.Item2} {n.Value.Item1}");
      }
      Console.WriteLine(string.Join(" ", o));
    }

    // Three coordinates: a node, its right neighbor, its bottom neighbor
    //Console.WriteLine("0 0 1 0 0 1");
  }

  private static (int, int)? FindNext(bool[,] arr, int si, int sj, int di, int dj)
  {
    si = si + di;
    sj = sj + dj;
    for (int i = si; i < arr.GetLength(0) && (di > 0 || i == si); i++)
    {
      for (int j = sj; j < arr.GetLength(1) && (dj > 0 || j == sj); j++)
      {
        if (arr[i, j])
          return (i, j);
      }
    }

    return null;
  }
}