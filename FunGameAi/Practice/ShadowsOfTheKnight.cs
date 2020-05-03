using System;

class PlayerBat
{
  static void MainBat()
  {
    var inputs = Console.ReadLine().Split(' ');
    int W = int.Parse(inputs[0]);
    int H = int.Parse(inputs[1]);
    Console.ReadLine();
    inputs = Console.ReadLine().Split(' ');
    int x0 = int.Parse(inputs[0]);
    int y0 = int.Parse(inputs[1]);

    int minX = 0, minY = 0, maxX = W - 1, maxY = H - 1;

    while (true)
    {
      var bombDir = Console.ReadLine();

      if (bombDir[0] == 'U')
        maxY = y0-1;

      if (bombDir[0] == 'D')
        minY = y0+1;

      if (bombDir.IndexOf('L')!=-1)
        maxX = x0-1;

      if (bombDir.IndexOf('R')!=-1)
        minX = x0+1;

      x0 = (maxX+minX) / 2;
      y0 = (maxY+minY) / 2;

      Console.WriteLine( x0+" "+y0);
    }
  }
}