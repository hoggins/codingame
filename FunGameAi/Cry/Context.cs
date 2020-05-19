using System;
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

  public void ReadInitInput()
  {
    Field.ReadInit();
  }

  public void TickUpdate()
  {
    ResetTick();
    ++Tick;
    ReadTickInput();
    PatchMap();
  }

  public Point? FindMineCell(Point fromPos)
  {
    var entity = Entities.Where(e=>e.Type == EntityType.Trap).FindMin(c=>Utils.Distance(c.Pos, fromPos));
    return entity?.Pos;
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

  private void ResetTick()
  {
    _visibleOre = null;
  }

  public void ReadTickInput()
  {
    var cx = this;
    string[] inputs;
    inputs = Console.ReadLine().Split(' ');
    cx.MyScore = int.Parse(inputs[0]); // Amount of ore delivered
    cx.OpponentScore = int.Parse(inputs[1]);

    Field.InputReadMap();

    inputs = Console.ReadLine().Split(' ');
    int entityCount = int.Parse(inputs[0]); // number of entities visible to you
    cx.RadarCooldown = int.Parse(inputs[1]); // turns left until a new radar can be requested
    cx.TrapCooldown = int.Parse(inputs[2]); // turns left until a new trap can be requested
    var updated = new HashSet<int>();
    for (int i = 0; i < entityCount; i++)
    {
      inputs = Console.ReadLine().Split(' ');
      var id = int.Parse(inputs[0]); // unique id of the entity
      var entity = cx.Entities.Find(e => e.Id == id);
      if (entity == null)
        cx.Entities.Add(entity = new Entity());
      entity.Read(inputs);
      updated.Add(entity.Id);
    }
    cx.Entities.RemoveAll(e => !updated.Contains(e.Id));
  }

  private void PatchMap()
  {
    foreach (var e in Entities)
    {
      if (e.Type == EntityType.Trap)
        Field.Map[e.Y, e.X].IsMined = true;
    }
  }

  private int CalcVisibleOre()
  {
    return Field.Map.EnumerateMap().Sum(c => !c.IsSafe() ? 0 : c.Ore.GetValueOrDefault());
  }
}
