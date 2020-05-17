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

      foreach (var state in _states)
      {
        RestoreField(state);
      }

      _states[0].Field.InitCellConnections();
      foreach (var s in _states.Skip(1))
        s.Field.CellConnections = _states[0].Field.CellConnections;

      TickSlider.Maximum = _states.Count - 1;
      TickSlider.ValueChanged += (sender, args) =>
      {
        var state = _states[(int) TickSlider.Value];

        GenerateForTick(state);
      };
      GenerationSlider.Maximum = 0;
      GenerationSlider.ValueChanged += (o, args) => DrawGeneration();
    }

    private void RestoreField(TheOutState state)
    {
      state.Field = new GameField
      {
        Grid = new Cell[state.Flags.GetLength(0),state.Flags.GetLength(1)]
      };
      for (int i = 0; i < state.Flags.GetLength(0); i++)
      {
        for (int j = 0; j < state.Flags.GetLength(1); j++)
        {
          state.Field.Grid[i,j].SetFlag((CellFlags) state.Flags[i,j]);
        }
      }
    }

    private void Simulate_OnClick(object sender, RoutedEventArgs e)
    {
      // TestUncheck();
      // return;

      var state = _states[(int) TickSlider.Value];

      GenerateForTick(state);


      // TestBest(sim, state);
    }

    private void GenerateForTick(TheOutState state)
    {
      var sim = new Simulator(state.Field.Height, state.Field.Width);
      Generations = sim.Run(state.Field, state.Pacs);

      var bestVal = Generations.Max(s => s.Sum(p => p.Geathered));
      GenerationSlider.Value = Generations.FindIndex(s => s.Sum(p => p.Geathered) == bestVal);

      GenerationSlider.Maximum = Generations.Count - 1;
      DrawGeneration();
    }

    private void TestUncheck()
    {
      var state = _states[(int) TickSlider.Value];

      var newField = new GameField();
      newField.CopyFrom(state.Field);


      newField.SetPac(new Pac(10) {Pos = new Point(27, 7), IsMine = true});

      // newField.Grid[7,27].SetPellet(0);

      // newField.Grid[9,27].SetPellet(0);

      // newField.WaveUncheck(27, 7);

      Drawer.DrawMap(new HeatMap(newField.CalcValue()));
      Drawer.DrawPacs(state.Pacs.Select(p=>p.Pos));
      SetImage(Drawer.Bmp);
    }

    private void TestBest(Simulator sim, TheOutState state)
    {
      var best = sim.RunBest(state.Field, state.Pacs);
      foreach (var path in best.Values)
      {
        Drawer.DrawPath(path, Color.Aqua);
      }

      SetImage(Drawer.Bmp);
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