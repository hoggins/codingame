using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace APackmanDebug
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow
  {
    private List<TheOut> _maps;

    private Drawer Drawer = new Drawer();

    public MainWindow()
    {
      InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);

      PrepareInput();
      DrawFrame(0);
      SetImage(Drawer.Bmp);

      Slider.Maximum = _maps.Count-1;
      Slider.ValueChanged += (sender, args) =>
      {
        var idx = (int)Slider.Value;
        DrawFrame(idx);
        SetImage(Drawer.Bmp);
      };
    }

    private void DrawPath_OnClick(object sender, RoutedEventArgs e)
    {
      var path = ParsePath(PathInput.Text);
      Drawer.DrawPath(path, Brushes.Blue);
      SetImage(Drawer.Bmp);
    }

    private void DrawFrame(int idx)
    {
      var item = _maps[idx];
      var total = item.Values.Subtract(item.Infl);
      var map = new HeatMap(total);

      Drawer.DrawMap(map);
      Drawer.DrawPacs(item.Pacs.Select(p=>p.Pos));

      foreach (var pred in item.Predictions)
      {
        foreach (var paths in pred.Path)
        {
          Drawer.DrawPath(paths[0], Brushes.CornflowerBlue);
          Drawer.DrawPath(paths[1], Brushes.Blue);
        }
      }
    }

    private void SetImage(Bitmap _bmp)
    {
      Image.Width = _bmp.Width;
      Image.Height = _bmp.Height;
      Image.Source = _bmp.BitmapToImageSource();
    }

    private void PrepareInput()
    {
      var data = new CGOutputProcessor("Data.txt");
      _maps = data.States.Select(s => (new SerialReader(s)).Read<TheOut>()).ToList();
    }

    private Path ParsePath(string str)
    {
      var parts = str.Split('>');
      var path = new Path(parts.Length);
      foreach (var part in parts)
      {
        if (part.Length == 0)
          continue;
        var xyStr = part.Trim().Split(' ');
        var x = int.Parse(xyStr[0].Substring(1));
        var y = int.Parse(xyStr[1]);
        path.Add(new Point(x, y));
      }

      return path;
    }


  }
}