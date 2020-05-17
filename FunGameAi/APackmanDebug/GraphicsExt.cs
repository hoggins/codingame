using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace APackmanDebug
{
  public static class GraphicsExt
  {
    public static void DrawLine(this Graphics g, Pen pen, Point f, Point t, int sc)
    {
      var sh = sc / 2;
      g.DrawLine(pen, f.X*sc + sh, f.Y*sc + sh, t.X*sc + sh, t.Y*sc + sh);
    }

    public static BitmapImage BitmapToImageSource(this Bitmap bitmap)
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