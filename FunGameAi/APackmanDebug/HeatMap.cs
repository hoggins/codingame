using System.Collections.Generic;
using System.Linq;

namespace APackmanDebug
{
  public class HeatMap
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
}