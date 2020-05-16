using System;
using System.Linq;
using System.Windows;

namespace APackmanDebug
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      // WriterTest.Test();
    }
  }

  class WriterTest
  {
    public static void Test()
    {

      var data = new CGOutputProcessor("Data.txt");
      foreach (var state in data.States)
      {
        var r = new SerialReader(state);
        var gf = r.Read<GameField>();
        Console.WriteLine();
      }

      var res = data.States.Select(s => (new SerialReader(s)).Read<GameField>()).ToList();


      Console.WriteLine();
    }
  }
}