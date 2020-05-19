using System;

public class CryField
{
  private readonly Context _cx;
  public MapCell[,] Map;
  public int Width;
  public int Height;

  public CryField(Context cx)
  {
    _cx = cx;
  }

  public MapCell GetCell(Point pos)
  {
    return Map[pos.Y, pos.X];
  }

  public void ReadInit()
  {
    string[] inputs;
    inputs = Console.ReadLine().Split(' ');
    Width = int.Parse(inputs[0]);
    Height = int.Parse(inputs[1]); // size of the map


    Map = new MapCell[Height, Width];

    for (int i = 0; i < Height; i++)
    for (int j = 0; j < Width; j++)
      Map[i,j] = new MapCell(new Point(j,i));
  }
}
