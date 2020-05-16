using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media.Imaging;

namespace APackmanDebug
{

  class HeatMap
  {
    private readonly float[,] _values;
    private float _max;
    private float _min;


    public int Height => _values.GetLength(0);
    public int Width => _values.GetLength(1);

    public HeatMap(float[,] values)
    {
      _values = values;

      _max = Enumerate().Max();
      _min = Enumerate().Min();
    }

    public float GetHeat(int i, int j)
    {
      var item = _values[i, j];
      if (item > 0)
        return item / _max;
      else
        return item / _min * -1;
    }

    private IEnumerable<float> Enumerate()
    {
      for (int i = 0; i < _values.GetLength(0); i++)
      {
        for (int j = 0; j < _values.GetLength(1); j++)
        {
          yield return _values[i, j];
        }
      }
    }
  }

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow
  {
    private List<TheOut> _maps;
    private const int Scale = 30;

    public MainWindow()
    {
      InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);

      PrepareInput();

      Slider.Maximum = _maps.Count-1;

      Slider.ValueChanged += (sender, args) =>
      {
        var item = _maps[(int)Slider.Value];

        var total = item.Values.Subtract(item.Infl);

        var map = new HeatMap(total);

        var bmp = DrawMap(map);

        DrawPacs(bmp, item.Pacs);

        Image.Width = bmp.Width;
        Image.Height = bmp.Height;
        Image.Source = BitmapToImageSource(bmp);
      };

    }

    private Bitmap DrawMap(HeatMap map)
    {
      var bmp = new Bitmap(map.Width*Scale, map.Height*Scale);
      using var g = Graphics.FromImage(bmp);
      // using var drawFont = new Font("Arial", 16);
      // using var drawBrush = new SolidBrush(Color.Red);

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
          // var s = ToWallStr(f);
          // g.DrawString(s, drawFont, drawBrush, j*scale, i*scale);
        }
      }

      return bmp;
    }

    private Bitmap DrawPacs(Bitmap bmp, List<Pac> pacs)
    {
      using var g = Graphics.FromImage(bmp);
      using var drawFont = new Font("Arial", 16);
      using var drawBrush = new SolidBrush(Color.Red);

      foreach (var pac in pacs)
      {
        if (!pac.IsMine)
          continue;
        g.DrawString("X", drawFont, drawBrush, pac.Pos.X*Scale, pac.Pos.Y*Scale);
      }

      return bmp;
    }

    private string ToWallStr(CellFlags f)
    {
      if (f.HasFlag(CellFlags.Wall))
      {
        return "#";
      }
      /*else if (f.HasFlag(CellFlags.Space))
      {
        return " ";
      }*/
      else if (f.HasFlag(CellFlags.Pellet))
      {
        return ".";
      }
      /*else if (f.HasFlag(CellFlags.Visited))
      {
      }*/
      else if (f.HasFlag(CellFlags.EnemyPac))
      {
        return "e";
      }
      else if (f.HasFlag(CellFlags.MyPac))
      {
        return "m";
      }
      else if (f.HasFlag(CellFlags.HadPellet))
      {
        return "x";
      }
      else if (f.HasFlag(CellFlags.Visible))
      {
        return " ";
      }
      else if (f.HasFlag(CellFlags.Seen))
      {
        return " ";
      }
      else if ((f & (~CellFlags.Seen)) == f)
      {
        return "-";
      }
      else
      {
        return "?";
      }
    }

    private void PrepareInput()
    {
      var data = new CGOutputProcessor("Data.txt");

      _maps = data.States.Select(s => (new SerialReader(s)).Read<TheOut>()).ToList();
    }

    BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
      using (MemoryStream memory = new MemoryStream())
      {
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        memory.Position = 0;
        BitmapImage bitmapimage = new BitmapImage();
        bitmapimage.BeginInit();
        bitmapimage.StreamSource = memory;
        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapimage.EndInit();

        return bitmapimage;
      }
    }
  }
}