using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Deliver more ore to hq (left side of the map) than your opponent. Use radars to find ore but beware of traps!
 **/
class Player
{
  public static void Print(string s) => Console.Error.WriteLine(s);

  static void Main(string[] args)
  {
    var cx = ReadInitInput();

    // game loop
    while (true)
    {
      ReadTickInput(cx);
      cx.PatchMap();

      HighOrderScout.TryGive(cx);
      HighOrderMine.TryGive(cx);

      foreach (var robot in cx.EnumerateRobots(true))
      {
        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");
        //var robot = cx.Entities.Find(e => e.Id == i);
        if (robot.IsDead)
        {
          Console.WriteLine("WAIT");
          continue;
        }

        if (!robot.IsBusy(cx))
        {
          robot.Order?.Finalize(cx);

          EOrder newOrder;
          var hasOrder = OrderHuntOre.TryProduce(cx, robot, out newOrder)
                         || OrderReturnOre.TryProduce(robot, out newOrder)
                         || OrderRandomDig.TryProduce(cx, robot, out newOrder)
            ;
          robot.Order = newOrder;
        }

        var command = robot.Order?.ProduceCommand(cx) ?? "WAIT";
        // WAIT|MOVE x y|DIG x y|REQUEST item
        Console.WriteLine(command);
      }
    }
  }

  public class HighOrderMine
  {
    private const int OreToStart = 10;

    private static (int, int)[] ScoutPoints = new[]
    {
      (6, 9),
      (11, 6),
      (16, 6),
      (16, 9),
      (21, 6),
      (21, 9),
    };

    public static void TryGive(Context cx)
    {
      if (cx.TrapCooldown > 0)
        return;

//      var isInProgress = cx.EnumerateRobots().Any(e => e.Order is OrderPlaceMine);
//      if (isInProgress)
//        return;

//      var visibleOre = cx.EnumerateMap().Sum(c => c.Ore.GetValueOrDefault());
//      if (visibleOre < OreToStart)
//        return;

      var point = GetNextPoint(cx);
      if (!point.HasValue)
        return;

      var robot = cx.EnumerateRobots()
        .Where(e => !e.IsBusy(cx) && e.Pos.Item1 == 0)
        .FindMin(e =>  Distance(e.Pos, point.Value));

      if (robot == null)
        return;

      robot.Order = new OrderPlaceMine(robot, point.Value);
    }

    public static (int, int)? GetNextPoint(Context cx)
    {
      foreach (var p in ScoutPoints)
      {
//        var closestMine = cx.FindMineCell(p);
//        if (closestMine.HasValue && Distance(closestMine.Value, p) < 5)
//          continue;

        var closestVein = cx.FindOreCell(p, 2);
        if (closestVein != null && Distance(closestVein.Pos, p) < 3)
          return closestVein.Pos;
      }

      return null;
    }
  }

  public class HighOrderScout
  {
    private const int OreToStart = 30;

    private static readonly (int, int)[] Points = new[]
    {
      (10, 7),
      (5, 4),
      (5, 12),
      (14, 3),
      (15, 11),
      (20, 8),
      (23, 2),
      (25, 10),
    };

    public static void TryGive(Context cx)
    {
      var isInProgress = cx.EnumerateRobots().Any(e => e.Order is OrderPlaceRadar);
      if (isInProgress)
        return;

      var visibleOre = cx.EnumerateMap().Sum(c => !c.IsSafe() ? 0 : c.Ore.GetValueOrDefault());
      if (visibleOre > OreToStart)
        return;

      var scoutPoint = GetNextScoutPoint(cx);
      if (!scoutPoint.HasValue)
        return;

      var robot = cx.EnumerateRobots()
        .Where(e => !e.IsBusy(cx) && e.Pos.Item1 == 0)
        .FindMin(e => Distance(e.Pos, scoutPoint.Value));

      if (robot == null)
      {
        // prevent hang when we have to switch to scout in field
        if (visibleOre == 0)
          robot = cx.EnumerateRobots()
            .Where(e => !e.IsBusy(cx, true))
            .FindMin(e => Distance(e.Pos, (0, e.Y)) + Distance(e.Pos, scoutPoint.Value));

        if (robot == null)
          return;
      }

      robot.Order = new OrderPlaceRadar(robot, scoutPoint.Value);
    }

    private static (int, int)? GetNextScoutPoint(Context cx)
    {
      foreach (var p in Points)
      {
        var taken = cx.Entities.Any(e => e.X == p.Item1 && e.Y == p.Item2 && e.Type == EntityType.Radar);
        if (taken)
          continue;
        return p;
      }

      return null;
    }
  }

  #region Input

  private static Context ReadInitInput()
  {
    string[] inputs;
    inputs = Console.ReadLine().Split(' ');
    int width = int.Parse(inputs[0]);
    int height = int.Parse(inputs[1]); // size of the map

    var cx = new Context();
    cx.Map = new MapCell[width, height];

    for (int i = 0; i < height; i++)
      for (int j = 0; j < width; j++)
        cx.Map[j,i] = new MapCell(j,i);

    return cx;
  }

  private static void ReadTickInput(Context cx)
  {
    string[] inputs;
    inputs = Console.ReadLine().Split(' ');
    int myScore = int.Parse(inputs[0]); // Amount of ore delivered
    int opponentScore = int.Parse(inputs[1]);
    InputReadMap(cx, cx.Map.GetLength(1), cx.Map.GetLength(0));

    inputs = Console.ReadLine().Split(' ');
    int entityCount = int.Parse(inputs[0]); // number of entities visible to you
    cx.RadarCooldown = int.Parse(inputs[1]); // turns left until a new radar can be requested
    cx.TrapCooldown = int.Parse(inputs[2]); // turns left until a new trap can be requested
    for (int i = 0; i < entityCount; i++)
    {
      inputs = Console.ReadLine().Split(' ');
      var id = int.Parse(inputs[0]); // unique id of the entity
      var entity = cx.Entities.Find(e => e.Id == id);
      if (entity == null)
        cx.Entities.Add(entity = new Entity());
      entity.Read(inputs);
    }
  }

  private static void InputReadMap(Context cx, int height, int width)
  {
    string[] inputs;
    for (int i = 0; i < height; i++)
    {
      inputs = Console.ReadLine().Split(' ');
      for (int j = 0; j < width; j++)
      {
        int? ore = inputs[2 * j] == "?" ? (int?) null : int.Parse(inputs[2 * j]); // amount of ore or "?" if unknown
        bool hole = int.Parse(inputs[2 * j + 1]) == 1; // 1 if cell has a hole
        cx.Map[j, i].Set(ore, hole);
      }
    }
  }

  #endregion

  public static int Distance((int, int) p1, (int, int) p2)
  {
    return Math.Abs(p1.Item1 - p2.Item1) + Math.Abs(p1.Item2 - p2.Item2);
  }
}

#region Base types

public class MapCell
{
  public (int, int) Pos;
  public int? Ore;
  public bool Hole;

  public bool IsMined;
  public bool IsDigged;

  public int DigLock;

  public MapCell(int x, int y)
  {
    Pos = (x, y);
  }

  public void Set(int? ore, bool hole)
  {
    Ore = ore;
    Hole = hole;
  }

  public bool IsSafe()
  {
    return !IsMined && (IsDigged || !Hole);
  }
}

public enum ItemType
{
  None = -1,
  Radar = 2,
  Trap = 3,
  Ore = 4,
}

public enum EntityType
{
  Robot = 0,
  Enemy,
  Radar,
  Trap
}

public abstract class EOrder
{
  public abstract bool IsCompleted(Context map, Entity robot);
  public abstract string ProduceCommand(Context cx);

  public virtual void Finalize(Context cx)
  {
  }
}

public class Entity
{
  public int Id;
  public EntityType Type;
  public int X;
  public int Y;
  public ItemType Item;

  public EOrder Order;

  public bool IsDead => X == -1 && Y == -1;
  public (int, int) Pos => (X, Y);

  public void Read(string[] inputs)
  {
    Id = int.Parse(inputs[0]); // unique id of the entity
    Type = (EntityType) int.Parse(inputs[1]); // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap
    X = int.Parse(inputs[2]);
    Y = int.Parse(inputs[3]); // position of the entity
    Item = (ItemType) int.Parse(
      inputs[4]); // if this entity is a robot, the item it is carrying (-1 for NONE, 2 for RADAR, 3 for TRAP, 4 for ORE)
  }

  public static string Move((int, int Y) target)
  {
    return $"MOVE {target.Item1} {target.Item2}";
  }

  public bool IsBusy(Context cx, bool ignoreLowPrio = false)
  {
    if (ignoreLowPrio && Order is OrderRandomDig)
      return false;

    return Order != null && !Order.IsCompleted(cx, this);
  }

  public static string Dig((int, int) target)
  {
    return $"DIG {target.Item1} {target.Item2}";
  }
}

public class Context
{
  public MapCell[,] Map;
  public List<Entity> Entities = new List<Entity>();
  public int RadarCooldown;
  public int TrapCooldown;

  public IEnumerable<Entity> EnumerateRobots(bool includeDead = false) =>
    Entities.Where(e => e.Type == EntityType.Robot && (includeDead && e.IsDead || !e.IsDead));

  public IEnumerable<MapCell> EnumerateMap()
  {
    for (int i = 0; i < Map.GetLength(0); i++)
    {
      for (int j = 0; j < Map.GetLength(1); j++)
      {
        yield return Map[i, j];
      }
    }
  }

  public MapCell GetCell((int, int) pos)
  {
    return Map[pos.Item1, pos.Item2];
  }

  public MapCell FindOreCell((int, int) fromPos, int minStack = 1)
  {
    return EnumerateMap()
      .Where(c=>c.Ore >= minStack && c.IsSafe())
      .FindMin(c=>CalcWeightedDist(fromPos, c));
  }

  private static int CalcWeightedDist((int, int) fromPos, MapCell c)
  {
    var crowdCoef = c.DigLock*2;
    var captureCoef = (c.IsDigged && c.Ore == 1 ? 3 : 0);
    var preventMinignCoef = (c.IsDigged && c.Ore == 2 ? -4 : 0);
    return Player.Distance(c.Pos, fromPos) + crowdCoef + captureCoef + preventMinignCoef;
  }

  public (int, int)? FindMineCell((int, int) fromPos)
  {
    var entity = Entities.Where(e=>e.Type == EntityType.Trap).FindMin(c=>Player.Distance(c.Pos, fromPos));
    return entity?.Pos;
  }

  public void PatchMap()
  {
    foreach (var e in Entities)
    {
      if (e.Type == EntityType.Trap)
        Map[e.X, e.Y].IsMined = true;
    }
  }

  public void IncDigLock((int, int) target)
  {
    var cell = GetCell(target);
    cell.DigLock += 1;
  }

  public void DecDigLock((int, int) target)
  {
    var cell = GetCell(target);
    cell.DigLock -= 1;
  }

  public void SetDigged((int, int) target)
  {
    Map[target.Item1, target.Item2].IsDigged = true;
  }
}

#endregion

public class OrderPlaceMine : EOrder
{
  private readonly Entity _robot;
  private (int, int)? _scoutPoint;
  private bool _isLocked;
  private bool _isRequested;
  private bool _isReceived;
  private bool _isBured;

  public OrderPlaceMine(Entity robot, (int, int) scoutPoint)
  {
    _robot = robot;
    _scoutPoint = scoutPoint;
  }

  public override bool IsCompleted(Context map, Entity robot)
  {
    return _isRequested && _isReceived && _isBured;
  }

  public override string ProduceCommand(Context cx)
  {
    if (!_isLocked && _scoutPoint.HasValue)
    {
      _isLocked = true;
      cx.IncDigLock(_scoutPoint.Value);
    }

    if (!_isRequested)
    {
      if (_robot.X == 0)
      {
        _isRequested = true;
        return "REQUEST TRAP";
      }

      return Entity.Move((0, _robot.Y));
    }

    if (_robot.Item == ItemType.Trap)
    {
      _isReceived = true;

      if (_scoutPoint.HasValue)
      {
        var distToTarget = Player.Distance(_robot.Pos, _scoutPoint.Value);
        if (distToTarget < 7)
        {
          var vein = cx.FindOreCell(_scoutPoint.Value, 2);
          if (vein != null
              && Player.Distance(_robot.Pos, vein.Pos) < distToTarget
              && (vein.Pos.Item1 != _scoutPoint.Value.Item1 || vein.Pos.Item2 != _scoutPoint.Value.Item2))
          {
            cx.DecDigLock(_scoutPoint.Value);
            cx.IncDigLock(vein.Pos);
          }
          else
            cx.DecDigLock(_scoutPoint.Value);

          _scoutPoint = vein?.Pos;
        }
      }

      if (!_scoutPoint.HasValue)
        _scoutPoint = Player.HighOrderMine.GetNextPoint(cx);

      if (!_scoutPoint.HasValue)
        return null;

      var (x, y) = _scoutPoint.Value;
      return $"DIG {x} {y}";
    }

    if (_isReceived)
    {
      if (!_isBured)
      {
        _isBured = true;
      }
    }

    return null;
  }
}

public class OrderPlaceRadar : EOrder
{
  private readonly Entity _robot;
  private readonly (int, int) _scoutPoint;
  private bool _isRequested;

  public OrderPlaceRadar(Entity robot, (int, int) scoutPoint)
  {
    _robot = robot;
    _scoutPoint = scoutPoint;
  }

  public override bool IsCompleted(Context map, Entity robot)
  {
    return _isRequested && robot.Item != ItemType.Radar;
  }

  public override string ProduceCommand(Context cx)
  {
    if (!_isRequested)
    {
      if (_robot.X == 0)
      {
        _isRequested = true;
        return "REQUEST RADAR";
      }

      return Entity.Move((0, _robot.Y));
    }

    var (x, y) = _scoutPoint;
    return $"DIG {x} {y}";
  }

  public override void Finalize(Context cx)
  {
    if (_robot.Item == ItemType.Ore)
      cx.SetDigged(_scoutPoint);
  }
}

public class OrderReturnOre : EOrder
{
  public readonly (int, int Y) Target;

  private OrderReturnOre((int, int Y) target)
  {
    Target = target;
  }

  public static bool TryProduce(Entity robot, out EOrder newOrder)
  {
    newOrder = null;
    if (robot.Item != ItemType.Ore)
      return false;
    newOrder = new OrderReturnOre((0, robot.Y));
    return true;
  }

  public override bool IsCompleted(Context map, Entity robot)
  {
    return robot.Item == ItemType.None;
  }

  public override string ProduceCommand(Context cx)
  {
    return Entity.Move(Target);
  }
}

public class OrderHuntOre : EOrder
{
  private readonly Entity _robot;
  private readonly (int, int) _target;

  private bool _isLockSet;

  public OrderHuntOre(Entity robot, (int, int) target)
  {
    _robot = robot;
    _target = target;
  }

  public static bool TryProduce(Context cx, Entity robot, out EOrder order)
  {
    order = null;
    if (robot.Item != ItemType.None)
      return false;
    var oreCell = cx.FindOreCell(robot.Pos);
    if (oreCell == null)
      return false;
    order = new OrderHuntOre(robot, oreCell.Pos);
    return true;
  }

  public override bool IsCompleted(Context cx, Entity robot)
  {
    if (robot.Item == ItemType.Ore)
      return true;
    var cell = cx.GetCell(_target);
    if (cell.Ore < 1 || !cell.IsSafe())
      return true;
    return false;
  }

  public override string ProduceCommand(Context cx)
  {
    if (!_isLockSet)
    {
      _isLockSet = true;
      cx.IncDigLock(_target);
    }
    return $"DIG {_target.Item1} {_target.Item2}";
  }

  public override void Finalize(Context cx)
  {
    if (_robot.Item == ItemType.Ore)
      cx.SetDigged(_target);
  }
}
public class OrderRandomDig : EOrder
{
  private readonly Entity _robot;
  private readonly (int, int) _target;

  private SubOrder _subOrder;

  public OrderRandomDig(Entity robot)
  {
    _robot = robot;
  }

  public static bool TryProduce(Context cx, Entity robot, out EOrder order)
  {
    order = null;
    if (robot.Item != ItemType.None)
      return false;
    order = new OrderRandomDig(robot);
    return true;
  }

  public override bool IsCompleted(Context cx, Entity robot)
  {
    if (robot.Item != ItemType.None)
      return true;
    var visibleOre = cx.EnumerateMap().Sum(c => !c.IsSafe() ? 0 : c.Ore.GetValueOrDefault());
    if (visibleOre > 3)
      return true;
    return false;
  }

  public override string ProduceCommand(Context cx)
  {
    if (_subOrder != null)
    {
      if (!_subOrder.IsCompleted(cx))
        return _subOrder.ProduceCommand(cx);
      _subOrder.Finalize(cx);
      _subOrder = null;
    }

    if (_robot.X == 0)
    {
      _subOrder = new SubOrderMove(_robot, (_robot.X + 4, _robot.Y));
      return _subOrder.ProduceCommand(cx);
    }

    var command = DigFromCurrentLoc(cx);
    if (command == null)
    {
      if (_robot.X + 2 >= 30)
        return null;
      _subOrder = new SubOrderMove(_robot, (_robot.X + 2, _robot.Y));
      return _subOrder.ProduceCommand(cx);
    }
    return null;
  }

  private string DigFromCurrentLoc(Context cx)
  {
    foreach (var pos in EnumeratePositions(_robot.Pos))
    {
      var dig = new SubOrderDig(_robot, pos);
      if (dig.IsCompleted(cx))
        continue;
      _subOrder = dig;
      return _subOrder.ProduceCommand(cx);
    }

    return null;
  }

  private IEnumerable<(int, int)> EnumeratePositions((int, int) from)
  {
    var (x, y) = from;
    if (x + 1 < 30)
      yield return (x + 1, y);
    yield return (x, y);
    if (y+1 < 15)
      yield return (x, y+1);
    if (y-1 > 0)
      yield return (x, y-1);
    yield return (x-1, y);
  }

  public override void Finalize(Context cx)
  {
    if (_robot.Item == ItemType.Ore)
      cx.SetDigged(_target);
  }
}

public abstract class SubOrder
{
  protected Entity Robot;

  public SubOrder(Entity robot)
  {
    Robot = robot;
  }

  public abstract bool IsCompleted(Context cx);
  public abstract string ProduceCommand(Context cx);

  public virtual void Finalize(Context cx)
  {

  }
}

public class SubOrderMove : SubOrder
{
  private readonly (int, int) _pos;

  public SubOrderMove(Entity robot, (int, int) pos) : base(robot)
  {
    _pos = pos;
  }

  public override bool IsCompleted(Context cx)
  {
    return Robot.X == _pos.Item1 && Robot.Y == _pos.Item2;
  }

  public override string ProduceCommand(Context cx)
  {
    return Entity.Move(_pos);
  }
}

public class SubOrderDig : SubOrder
{
  private readonly (int, int) _pos;

  public SubOrderDig(Entity robot, (int, int) pos) : base(robot)
  {
    _pos = pos;
  }

  public override bool IsCompleted(Context cx)
  {
    var cell = cx.GetCell(_pos);
    return Robot.Item == ItemType.Ore || cell.Hole || !cell.IsSafe();
  }

  public override string ProduceCommand(Context cx)
  {
    return Entity.Dig(_pos);
  }

  public override void Finalize(Context cx)
  {
    var cell = cx.GetCell(_pos);
    if (Robot.Item == ItemType.Ore && cell.IsDigged)
      cell.IsDigged = true;
  }
}

static class Extensions
{
  public static T FindMin<T, TValue>(this IEnumerable<T> list, Func<T, TValue> predicate)
    where TValue : IComparable<TValue>
  {
    T result = list.FirstOrDefault();
    if (result != null)
    {
      var bestMin = predicate(result);
      foreach (var item in list.Skip(1))
      {
        var v = predicate(item);
        if (v.CompareTo(bestMin) < 0)
        {
          bestMin = v;
          result = item;
        }
      }
    }

    return result;
  }

  public static T FindMax<T, TValue>(this IEnumerable<T> list, Func<T, TValue> predicate)
    where TValue : IComparable<TValue>
  {
    T result = list.FirstOrDefault();
    if (result != null)
    {
      var bestMax = predicate(result);
      foreach (var item in list.Skip(1))
      {
        var v = predicate(item);
        if (v.CompareTo(bestMax) > 0)
        {
          bestMax = v;
          result = item;
        }
      }
    }

    return result;
  }
}