using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace APackmanDebug
{
  public class Drawer
  {
    public Bitmap Bmp { get; set; }
    private const int Scale = 30;

    public Bitmap DrawMap(HeatMap map)
    {
      Bmp = new Bitmap(map.Width*Scale, map.Height*Scale);
      using var g = Graphics.FromImage(Bmp);

      for (int i = 0; i < map.Height; i++)
      {
        for (int j = 0; j < map.Width; j++)
        {
          var f = map.GetHeat(i, j);

          if (f > 0)
          {
            using (var p = new SolidBrush(Color.FromArgb(0, (int)(255 * f), 0)))
              g.FillRectangle(p, j*Scale, i*Scale, Scale, Scale);
          }
          else if (f < 0)
          {
            using (var p = new SolidBrush(Color.FromArgb((int)(-255 * f), 0, 0)))
              g.FillRectangle(p, j*Scale, i*Scale, Scale, Scale);
          }
        }
      }

      return Bmp;
    }

    public void DrawPath(Path path, Brush brush)
    {
      var shift = Scale / 2;
      using var g = Graphics.FromImage(Bmp);
      using var pen = new Pen(brush, Scale / 6);
      for (var i = 1; i < path.Count; i++)
      {
        var fromP = path[i - 1];
        var toP = path[i];

        if (fromP.X - toP.X > 2)
          toP = new Point(fromP.X+1,fromP.Y);
        else if (fromP.X - toP.X < -2)
          toP = new Point(fromP.X-1,fromP.Y);

        g.DrawLine(pen, fromP, toP, Scale);
      }

      using var drawFont = new Font("Arial", 14);
      using var drawBrush = new SolidBrush(Color.BlueViolet);

      var pos = path.Last();
      // var weight = path.Value / path.Count;
      var weight = path.Value ;
      g.DrawString(weight.ToString("0.0"), drawFont, drawBrush, pos.X*Scale, pos.Y*Scale);
    }

    public void DrawPacs(IEnumerable<Point> pacs)
    {
      using var g = Graphics.FromImage(Bmp);
      using var drawFont = new Font("Arial", 21);
      using var drawBrush = new SolidBrush(Color.Aqua);

      foreach (var pac in pacs)
      {
        g.DrawString("X", drawFont, drawBrush, pac.X*Scale, pac.Y*Scale);
      }
    }
  }
}