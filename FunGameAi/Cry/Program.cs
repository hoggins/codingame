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
          EOrder newOrder;
          var hasOrder = OrderHuntOre.TryProduce(cx, robot, out newOrder)
                         || OrderReturnOre.TryProduce(robot, out newOrder)
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

      var isInProgress = cx.EnumerateRobots().Any(e => e.Order is OrderPlaceMine);
      if (isInProgress)
        return;

      var visibleOre = cx.EnumerateMap().Sum(c => c.Ore.GetValueOrDefault());
//      if (visibleOre < OreToStart)
//        return;

      var point = GetNextPoint(cx);
      if (!point.HasValue)
        return;

      var robot = cx.EnumerateRobots()
        .Where(e => !e.IsBusy(cx))
        .FindMin(e => e.Pos.Item1 == 0
          ? Distance(e.Pos, point.Value)
          : Distance(e.Pos, (0, e.Y)) + Distance(e.Pos, point.Value));

      if (robot == null)
        return;

      robot.Order = new OrderPlaceMine(robot, point.Value);
    }

    public static (int, int)? GetNextPoint(Context cx)
    {
      foreach (var p in ScoutPoints)
      {
        var closestMine = cx.FindMineCell(p);
        if (closestMine.HasValue && Distance(closestMine.Value, p) < 5)
          continue;

        var closestVein = cx.FindOreCell(p, 2);
        if (closestVein != null && Distance(closestVein.Pos, p) < 3)
          return closestVein.Pos;
      }

      return null;
    }
  }

  public class HighOrderScout
  {
    private const int OreToStart = 10;

    private static readonly (int, int)[] Points = new[]
    {
      (5, 10),
      (10, 5),
      (15, 5),
      (15, 10),
      (20, 5),
      (20, 10),
      (25, 5),
      (25, 10),
    };

    public static void TryGive(Context cx)
    {
      var isInProgress = cx.EnumerateRobots().Any(e => e.Order is OrderPlaceRadar);
      if (isInProgress)
        return;

      var visibleOre = cx.EnumerateMap().Sum(c => c.IsMined ? 0 : c.Ore.GetValueOrDefault());
      if (visibleOre > OreToStart)
        return;

      var scoutPoint = GetNextScoutPoint(cx);
      if (!scoutPoint.HasValue)
        return;

      var robot = cx.EnumerateRobots()
        .Where(e => !e.IsBusy(cx))
        .FindMin(e => e.Pos.Item1 == 0
          ? Distance(e.Pos, scoutPoint.Value)
          : Distance(e.Pos, (0, e.Y)) + Distance(e.Pos, scoutPoint.Value));

      if (robot == null)
        return;

      robot.Order = new OrderPlaceRadar(robot, scoutPoint.Value);
    }

    private static (int, int)? GetNextScoutPoint(Context cx)
    {
      foreach (var p in Points)
      {
        var entity = cx.Entities.Find(e => e.X == p.Item1 && e.Y == p.Item2);
        if (entity?.Type == EntityType.Radar)
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

  public MapCell(int x, int y)
  {
    Pos = (x, y);
  }

  public void Set(int? ore, bool hole)
  {
    Ore = ore;
    Hole = hole;
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

  public bool IsBusy(Context cx)
  {
    return Order != null && !Order.IsCompleted(cx, this);
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

  public MapCell FindOreCell((int, int) fromPos, int minStack = 1)
  {
    return EnumerateMap().Where(c=>!c.IsMined && c.Ore >= minStack).FindMin(c=>Player.Distance(c.Pos, fromPos));
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
}

#endregion

public class OrderPlaceMine : EOrder
{
  private readonly Entity _robot;
  private (int, int)? _scoutPoint;
  private bool _isRequested;
  private bool _isReceived;
  private bool _isBured;
  private bool _isCovered;

  public OrderPlaceMine(Entity robot, (int, int) scoutPoint)
  {
    _robot = robot;
    _scoutPoint = scoutPoint;
  }

  public override bool IsCompleted(Context map, Entity robot)
  {
    return _isRequested && _isReceived && _isBured /*&& _isCovered*/;
  }

  public override string ProduceCommand(Context cx)
  {
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

      if (_scoutPoint.HasValue && Player.Distance(_robot.Pos, _scoutPoint.Value) < 4)
      {
        var vein = cx.FindOreCell(_scoutPoint.Value, 2);
        _scoutPoint = vein?.Pos;
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
        Player.Print("placed");
        _isBured = true;
      }
    }

    Player.Print("unreachable trap");
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
    return _isRequested && robot.Item == ItemType.None;
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
  public (int, int) Target;

  public OrderHuntOre((int, int) target)
  {
    Target = target;
  }

  public static bool TryProduce(Context cx, Entity robot, out EOrder order)
  {
    order = null;
    if (robot.Item != ItemType.None)
      return false;
    var oreCell = cx.FindOreCell(robot.Pos);
    if (oreCell == null)
      return false;
    order = new OrderHuntOre(oreCell.Pos);
    return true;
  }

  public override bool IsCompleted(Context cx, Entity robot)
  {
    if (robot.Item != ItemType.None)
      return true;
    var cell = cx.Map[Target.Item1, Target.Item2];
    if (cell.Ore < 1 || cell.IsMined)
      return true;
    return false;
  }

  public override string ProduceCommand(Context cx)
  {
    return $"DIG {Target.Item1} {Target.Item2}";
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