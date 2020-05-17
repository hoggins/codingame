using System.Collections.Generic;
using System.Linq;


public class InflMap
{
  public Map<float> CostMap;

  public void Init(Context cx)
  {
    var g = cx.Field;
    CostMap = new Map<float>(g.Height, g.Width);
  }

  public void TickUpdate(Context cx)
  {
    CostMap.Clean();

    foreach (var pac in cx.Pacs)
    {
      if (!pac.IsMine)
        continue;
      var pos = pac.Pos;
      PlacePac(cx, pos);
    }
  }

  public void PlacePac(Context cx, Point pos)
  {
    var points = cx.Field.CellConnections[pos];
    var cost = 1f;
    foreach (var bundle in points)
    {
      foreach (var point in bundle)
      {
        CostMap[point] += cost;
      }

      cost -= 0.1f;
      if (cost < 0)
        break;
    }
  }
}
public class SimInflMap
{
  private readonly GameField _field;
  public Map<ushort> BaseCost;
  public Map<float> CostMap;

  public SimInflMap(GameField field, Map<ushort> baseCost, int h, int w)
  {
    _field = field;
    BaseCost = baseCost;
    var g = field;
    CostMap = new Map<float>(h,w);
  }

  public void Clean()
  {
    CostMap.Clean();
  }

  public void PlacePacs(List<SimPac> pacs)
  {


    foreach (var pac in pacs)
    {
      var pos = pac.Pos;
      PlacePac(_field, pos);
    }
  }

  public void Update()
  {
    CostMap.Clean();
    CostMap.Grid.Add(BaseCost.Grid);
  }

  public void PlacePac(GameField field, Point pos)
  {
    var points = field.CellConnections[pos];
    var cost = 1f;
    foreach (var bundle in points)
    {
      foreach (var point in bundle)
      {
        CostMap[point] += cost;
      }

      cost -= 0.1f;
      if (cost < 0)
        break;
    }
  }
}