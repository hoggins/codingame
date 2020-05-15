using System;

// note: putting structs in map will result in copying on access with indexers
public class Map<T>
{
  public readonly T[] Grid;
  public readonly int Height;
  public readonly int Width;

  public Map(int height, int width)
  {
    Height = height;
    Width = width;
    Grid = new T[height * width];
  }

  public int Length => Height * Width;

  public T this[Point p]
  {
    get { return Grid[p.ToIdx(Width)]; }
    set { Grid[p.ToIdx(Width)] = value; }
  }

  public T this[ushort y, ushort x]
  {
    get => Grid[y * Width + x];
    set => Grid[y * Width + x] = value;
  }

  public T this[int y, int x]
  {
    get => Grid[y * Width + x];
    set => Grid[y * Width + x] = value;
  }

  public void Clean()
  {
    Array.Clear(Grid, 0, Grid.Length);
  }

  public void Dump()
  {
    for (int i = 0; i < Height; i++)
    {
      for (int j = 0; j < Width; j++)
      {
        var idx = i * Width + j;
        Console.Error.Write(Grid[idx].ToString());
      }
      Console.Error.WriteLine();
    }
  }

  public void Dump(Func<T, string> toString)
  {
    for (int i = 0; i < Height; i++)
    {
      for (int j = 0; j < Width; j++)
      {
        var idx = i * Width + j;
        Console.Error.Write(toString(Grid[idx]));
      }
      Console.Error.WriteLine();
    }
  }
}