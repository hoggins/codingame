using System.Collections.Generic;
using System.Linq;

public class Context
{
  public CryModel Model;

  public int Tick;
  public List<Entity> Entities = new List<Entity>();
  public int RadarCooldown;
  public int TrapCooldown;
  public int MyScore;
  public int OpponentScore;

  public CryField Field;

  public int VisibleOre => (int) (_visibleOre ?? (_visibleOre = CalcVisibleOre()));

  private int? _visibleOre;

  public Context()
  {
    Model = new CryModel(this);
    Field = new CryField(this);
  }

  public IEnumerable<Entity> EnumerateRobots(bool includeDead = false) =>
    Entities.Where(e => e.Type == EntityType.Robot && (includeDead && e.IsDead || !e.IsDead));

  public Point? FindMineCell(Point fromPos)
  {
    var entity = Entities.Where(e=>e.Type == EntityType.Trap).FindMin(c=>Utils.Distance(c.Pos, fromPos));
    return entity?.Pos;
  }

  public void PatchMap()
  {
    foreach (var e in Entities)
    {
      if (e.Type == EntityType.Trap)
        Field.Map[e.X, e.Y].IsMined = true;
    }
  }

  public void IncDigLock(Point target)
  {
    var cell = Field.GetCell(target);
    cell.DigLock += 1;
  }

  public void DecDigLock(Point target)
  {
    var cell = Field.GetCell(target);
    cell.DigLock -= 1;
  }

  public void SetDigged((int, int) target)
  {
    Field.Map[target.Item1, target.Item2].IncreaseDig();
  }

  public void ResetTick()
  {
    _visibleOre = null;
  }

  private int CalcVisibleOre()
  {
    return Field.Map.EnumerateMap().Sum(c => !c.IsSafe() ? 0 : c.Ore.GetValueOrDefault());
  }

  public void ReadInitInput()
  {
    Field.ReadInit();

  }
}
