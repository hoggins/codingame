using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;

namespace APackmanDebug
{
  public partial class SimulatorWnd : Window
  {
    private List<TheOutState> _states;
    private List<List<SimPac>> Generations;
    private Drawer Drawer = new Drawer();

    public SimulatorWnd()
    {
      InitializeComponent();

    }

    protected override void OnActivated(EventArgs e)
    {
      base.OnActivated(e);
      var data = new CGOutputProcessor("DataStates.txt");
      _states = data.States.Select(s => (new SerialReader(s)).Read<TheOutState>()).ToList();

      _states[0].Field.InitCellConnections();
      foreach (var s in _states.Skip(1))
        s.Field.CellConnections = _states[0].Field.CellConnections;

      TickSlider.Maximum = _states.Count - 1;
      GenerationSlider.Maximum = 0;
      GenerationSlider.ValueChanged += (o, args) => DrawGeneration();
    }

    private void Simulate_OnClick(object sender, RoutedEventArgs e)
    {
      var state = _states[(int) TickSlider.Value];

      var sim = new Simulator(state.Field.Height, state.Field.Width);
      Generations = sim.Run(state.Field, state.Pacs);

      GenerationSlider.Maximum = Generations.Count - 1;


      DrawGeneration();

      /*var best = sim.RunBest(state.Field, state.Pacs);
      foreach (var path in best.Values)
      {
        Drawer.DrawPath(path,Brushes.Aqua);
      }
      SetImage(Drawer.Bmp);*/
    }

    private Color[] BrushBundle = {Color.Aquamarine, Color.Gold, Color.Magenta, Color.Cyan, Color.Salmon,};

    private void DrawGeneration()
    {
      var state = _states[(int) TickSlider.Value];
      var gen = Generations[(int)GenerationSlider.Value];

      Drawer.DrawMap(new HeatMap(state.Field.CalcValue()));

      // Drawer.DrawPacs(gen.Select(p=>p.Pos));

      var i = 0;
      foreach (var pac in gen)
      {
        var color = BrushBundle[++i%BrushBundle.Length];
        Drawer.DrawPath(pac.LastPath, color);
        Drawer.DrawText(pac.LastPath.First(), "O", color);
        Drawer.DrawText(pac.LastPath.Last(), "X", color);
      }

      var stats = new StringBuilder();
      var countByVal = Generations
        .GroupBy(g => g.Sum(p => p.Geathered))
        .OrderByDescending(g=>g.Key)
        .Select(g => $"{g.Key:0.0}:{g.Count()}");
      stats.AppendLine($"value " + string.Join(", ", countByVal));
      stats.AppendLine($"collect: " + gen.Sum(p => p.Geathered));
      GenStats.Text = stats.ToString();

      SetImage(Drawer.Bmp);
    }

    private void SetImage(Bitmap _bmp)
    {
      Image.Width = _bmp.Width;
      Image.Height = _bmp.Height;
      Image.Source = _bmp.BitmapToImageSource();
    }
  }
}