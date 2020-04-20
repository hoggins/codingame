﻿using System;
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
      cx.ResetTick();
      ++cx.Tick;
      ReadTickInput(cx);
      cx.PatchMap();

      HighOrderScout.TrySchedule(cx);
      var mineTaken = HighOrderHideScout.TryGive(cx);
      HighOrderScout.TryGive(cx);
      if (!mineTaken)
        HighOrderMine.TryGive(cx);

      foreach (var robot in cx.EnumerateRobots(true))
      {
        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");
        //var robot = cx.Entities.Find(e => e.Id == i);
        if (robot.IsDead)
        {
          Console.WriteLine("WAIT x");
          continue;
        }

        if (!robot.IsBusy(cx))
        {
          robot.Order?.Finalize(cx);

          EOrder newOrder;
          var hasOrder = TryProduceTakeRadar(cx, robot, out newOrder)
                         || TryProduceDigOre(cx, robot, out newOrder)
                         || OrderReturnOre.TryProduce(robot, out newOrder)
                         || OrderRandomDig.TryProduce(cx, robot, out newOrder)
            ;
          robot.Order = newOrder;
        }

        var command = (robot.Order?.ProduceCommand(cx) ?? "WAIT ")
                      + " " + robot.GetOrderName()
          ;
        // WAIT|MOVE x y|DIG x y|REQUEST item
        Console.WriteLine(command);
      }
    }
  }

  public static bool TryProduceTakeRadar(Context cx, Entity robot, out EOrder order)
  {
    order = null;
    if (cx.RadarCooldown > 0 || robot.X != 0 || cx.VisibleOre < Constant.StartRadarSpam)
      return false;

    if (robot.Item != ItemType.None)
      return false;

    cx.RadarCooldown = 6;
    order = new OrderTake(robot, ItemType.Radar);
    return true;
  }

  public static bool TryProduceDigOre(Context cx, Entity robot, out EOrder order)
  {
    order = null;
    if (robot.Item == ItemType.Ore)
      return false;
    var oreCell = cx.FindOreCell(robot.Pos);
    if (oreCell == null)
      return false;
    cx.IncDigLock(oreCell.Pos);
    order = new OrderDigOre(robot, oreCell.Pos);
    return true;
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

      if (cx.VisibleOre < 12)
        return;

      if (cx.MyScore  < cx.OpponentScore)
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

  public class HighOrderHideScout
  {
    public static bool TryGive(Context cx)
    {
      var point = HighOrderScout.PointToCover;
      if (cx.TrapCooldown > 0 || !point.HasValue)
        return false;

      HighOrderScout.PointToCover = null;

//      var r = cx.EnumerateRobots().FirstOrDefault(x=>!x.IsBusy(cx));
      var r = cx.EnumerateRobots()
        .Where(e => !e.IsBusy(cx) && e.Pos.Item1 == 0)
        .FindMin(e => Distance(e.Pos, point.Value));

      if (r == null)
        return false;

      var cell = cx.FindNearestSafe(point.Value);

      var newOrder = new OrderChain(r,
        new EOrder[]
        {
          //new OrderMove(r, (0, r.Y)),
          new OrderTake(r, ItemType.Trap),
          new OrderDigNearest(r, cell.Pos),
        }
      );
      r.Order = newOrder;
      return true;
    }
  }

  public class HighOrderScout
  {
    private const int OreToStart = 15;

    private static readonly (int, int)[] Points = new[]
    {
      (5, 4),
      (5, 12),
      (10, 7),
      (14, 3),
      (15, 11),
      (20, 8),
      (23, 2),
      (25, 10),
    };

    public static (int, int)? PointToCover { get; set; }
    private static ((int, int), int)? PointToPlace { get; set; }

    public static List<((int, int), int?)> MyRadars = new List<((int, int), int?)>();

    public static void TrySchedule(Context cx)
    {
      var isInProgress = cx.EnumerateRobots().Any(e => e.HasOrder<OrderDigNearestRadar>());
      if (isInProgress)
        return;

      var visibleOre = cx.VisibleOre;
      if (visibleOre > OreToStart)
        return;

      var safePoint = GetNextScoutPoint(cx);
      if (!safePoint.HasValue)
        return;

      var (scoutPoint, id) = safePoint.Value;
      if (!PickRobot(cx, scoutPoint, visibleOre, out var robot))
        return;

      if (id == 2 || id == 5 || id == 6)
        PointToCover = scoutPoint;
      PointToPlace = safePoint;

    }

    public static void TryGive(Context cx)
    {
      if (!PointToPlace.HasValue)
        return;
      var (scoutPoint, id) = PointToPlace.Value;

      var visibleOre = cx.VisibleOre;
      if (!PickRobot(cx, scoutPoint, visibleOre, out var robot))
        return;

      PointToPlace = null;
//      robot.Order = new OrderPlaceRadar(robot, scoutPoint, id);
      var newOrder = new OrderChain(robot,
        new EOrder[]
        {
          new OrderMove(robot, (0, robot.Y)),
          new OrderTake(robot, ItemType.Radar),
          new OrderDigNearestRadar(robot, scoutPoint, id),
        }
      );
      robot.Order = newOrder;
    }

    private static ((int, int), int)? GetNextScoutPoint(Context cx)
    {
      for (var i = MyRadars.Count - 1; i >= 0; i--)
      {
        var (pos, id) = MyRadars[i];
        var exist = cx.Entities.Any(e => e.X == pos.Item1 && e.Y == pos.Item2 && e.Type == EntityType.Radar);
        if (!exist)
          MyRadars.RemoveAt(i);
      }

      for (var i = 0; i < Points.Length; i++)
      {
        var myRadar = MyRadars.Find(r => r.Item2.HasValue && r.Item2.Value == i);
        if (myRadar.Item2.HasValue)
          continue;
        var p = Points[i];
        var cell = cx.GetCell(p);
        if (!cell.IsSafe())
        {
          var c = cx.FindNearestSafe(p);
          return c == null ? default : (c.Pos, i);
        }

        return (p, i);
      }

      return null;
    }

    private static bool PickRobot(Context cx, (int, int) scoutPoint, int visibleOre, out Entity robot)
    {
      robot = cx.EnumerateRobots()
        .Where(e => !e.IsBusy(cx) && e.Pos.Item1 == 0)
        .FindMin(e => Distance(e.Pos, scoutPoint));

      if (robot == null)
      {
        // prevent hang when we have to switch to scout in field
        if (visibleOre == 0)
          robot = cx.EnumerateRobots()
            .Where(e => !e.IsBusy(cx, true))
            .FindMin(e => Distance(e.Pos, (0, e.Y)) + Distance(e.Pos, scoutPoint));

        if (robot == null)
          return false;
      }

      return true;
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
    cx.MyScore = int.Parse(inputs[0]); // Amount of ore delivered
    cx.OpponentScore = int.Parse(inputs[1]);
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

class Constant
{
  public const int StartRadarSpam = 16;
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

  public int Distance((int, int) point)
  {
    return Player.Distance(Pos, point);
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
  protected Entity Robot;

  protected bool IsInitialized;

  protected EOrder(Entity robot)
  {
    Robot = robot;
  }

  public abstract bool IsCompleted(Context cx);

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
  public string Message { get; set; }

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

    return Order != null && !Order.IsCompleted(cx);
  }

  public static string Dig((int, int) target)
  {
    return $"DIG {target.Item1} {target.Item2}";
  }

  public static string Take(ItemType item)
  {
    switch (item)
    {
      case ItemType.Radar: return "REQUEST RADAR";
      case ItemType.Trap: return "REQUEST TRAP";
      default:
        throw new ArgumentOutOfRangeException(nameof(item), item, null);
    }
  }

  public string GetOrderName()
  {
    if (Order is OrderChain chain)
      return "ch " + chain.GetOrderName();
    return Order?.GetType().Name ?? "n";
  }

  public bool HasOrder<T>()
  {
    return Order is T || (Order is OrderChain chain) && chain.HasOrder<T>();
  }
}

public class Context
{
  public int Tick;
  public MapCell[,] Map;
  public List<Entity> Entities = new List<Entity>();
  public int RadarCooldown;
  public int TrapCooldown;
  public int MyScore;
  public int OpponentScore;

  public int VisibleOre => (int) (_visibleOre ?? (_visibleOre = CalcVisibleOre()));

  private int? _visibleOre;

  public IEnumerable<Entity> EnumerateRobots(bool includeDead = false) =>
    Entities.Where(e => e.Type == EntityType.Robot && (includeDead && e.IsDead || !e.IsDead));

  public IEnumerable<MapCell> EnumerateMap()
  {
    for (int j = 0; j < Map.GetLength(1); j++)
    {
      for (int i = 0; i < Map.GetLength(0); i++)
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
    var nearest = EnumerateMap()
      .Where(c=>c.Ore >= minStack && c.IsSafe())
      .FindMin(c=>CalcWeightedDist(fromPos, c));
    var leftMost = EnumerateMap()
      .Where(c => c.Ore >= minStack && c.IsSafe())
      .FirstOrDefault();

    if (leftMost != null && nearest.Pos.Item1 - leftMost.Pos.Item1 > 5)
      return leftMost;
    return nearest;
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

  public MapCell FindNearestSafe((int, int) fromPos)
  {
    for (int i = 1; i < 29; i++)
    {
      foreach (var pos in Utils.EnumerateNeighbors(fromPos, i))
      {
        var cell = GetCell(pos);
        if (cell.IsSafe())
          return cell;
      }
    }

    return null;
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

  public void ResetTick()
  {
    _visibleOre = null;
  }

  private int CalcVisibleOre()
  {
    return EnumerateMap().Sum(c => !c.IsSafe() ? 0 : c.Ore.GetValueOrDefault());
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

  public OrderPlaceMine(Entity robot, (int, int) scoutPoint) : base(robot)
  {
    _robot = robot;
    _scoutPoint = scoutPoint;
  }

  public override bool IsCompleted(Context map)
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

    if (_robot.Item != ItemType.Trap && !_isReceived)
      return "REQUEST TRAP";


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

public class OrderReturnOre : EOrder
{
  public readonly (int, int Y) Target;

  private OrderReturnOre(Entity robot, (int, int Y) target) : base(robot)
  {
    Target = target;
  }

  public static bool TryProduce(Entity robot, out EOrder newOrder)
  {
    newOrder = null;
    if (robot.Item != ItemType.Ore)
      return false;
    newOrder = new OrderReturnOre(robot, (0, robot.Y));
    return true;
  }

  public override bool IsCompleted(Context map)
  {
    return Robot.Item == ItemType.None;
  }

  public override string ProduceCommand(Context cx)
  {
    return Entity.Move(Target);
  }
}

public class OrderHuntOre : EOrder
{
  private readonly (int, int) _target;

  private bool _isLockSet;

  public OrderHuntOre(Entity robot, (int, int) target) : base(robot)
  {
    _target = target;
  }

  public override bool IsCompleted(Context cx)
  {
    if (Robot.Item == ItemType.Ore)
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
    if (Robot.Item == ItemType.Ore)
      cx.SetDigged(_target);
  }
}

public class OrderChain : EOrder
{
  private readonly List<EOrder> _orders;

  public OrderChain(Entity robot, EOrder[] orders) : base(robot)
  {
    _orders = orders.ToList();
  }

  public override bool IsCompleted(Context cx)
  {
    return !_orders.Any() || _orders.All(o => o.IsCompleted(cx));
  }

  public override string ProduceCommand(Context cx)
  {
    var subOrder = _orders[0];
    if (!subOrder.IsCompleted(cx))
      return subOrder.ProduceCommand(cx);

    subOrder.Finalize(cx);
    _orders.RemoveAt(0);

    while (_orders.Count > 0)
    {
      var nextOrder = _orders[0];
      if (!nextOrder.IsCompleted(cx))
        return nextOrder.ProduceCommand(cx);

      // todo call finalize for skipped orders?
      subOrder.Finalize(cx);
      _orders.RemoveAt(0);
    }

    return null;
  }

  public override void Finalize(Context cx)
  {
    base.Finalize(cx);
    foreach (var order in _orders)
    {
      order.Finalize(cx);
    }
  }

  public string GetOrderName()
  {
    return _orders.FirstOrDefault()?.GetType().Name ?? "n";
  }

  public bool HasOrder<T>()
  {
    return _orders.Any(o=>o is T);
  }
}

public class OrderRandomDig : EOrder
{
  private readonly (int, int) _target;

  private EOrder _subOrder;

  public OrderRandomDig(Entity robot) : base(robot)
  {
  }

  public static bool TryProduce(Context cx, Entity robot, out EOrder order)
  {
    order = null;
    if (robot.Item != ItemType.None)
      return false;
    order = new OrderRandomDig(robot);
    return true;
  }

  public override bool IsCompleted(Context cx)
  {
    if (Robot.Item != ItemType.None)
      return true;
    if (cx.VisibleOre > 3)
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

    if (Robot.X == 0)
    {
      return Shift(cx, 4);
    }

    var command = DigFromCurrentLoc(cx);
    if (command != null)
      return command;
    if (Robot.X + 2 >= 30)
      return null;
    return Shift(cx, 2);
  }

  private string Shift(Context cx, int v)
  {
    _subOrder = new OrderDigOre(Robot, (Robot.X + v, Robot.Y));
    if (_subOrder.IsCompleted(cx))
      _subOrder = new OrderMove(Robot, (Robot.X + v, Robot.Y));
    return _subOrder.ProduceCommand(cx);
  }

  private string DigFromCurrentLoc(Context cx)
  {
    foreach (var pos in Utils.EnumerateNeighbors(Robot.Pos))
    {
      var dig = new OrderDigOre(Robot, pos);
      if (dig.IsCompleted(cx))
        continue;
      _subOrder = dig;
      return _subOrder.ProduceCommand(cx);
    }

    return null;
  }

  public override void Finalize(Context cx)
  {
    if (Robot.Item == ItemType.Ore)
      cx.SetDigged(_target);
  }
}

public class OrderMove : EOrder
{
  private readonly (int, int) _pos;

  public OrderMove(Entity robot, (int, int) pos) : base(robot)
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


public abstract class OrderDig : EOrder
{
  protected (int, int) Pos;

  public OrderDig(Entity robot, (int, int) pos) : base(robot)
  {
    Pos = pos;
  }

  public override string ProduceCommand(Context cx)
  {
    return Entity.Dig(Pos);
  }

  public override void Finalize(Context cx)
  {
    var cell = cx.GetCell(Pos);
    if (Robot.Item == ItemType.Ore && cell.Hole)
      cell.IsDigged = true;
  }
}

public class OrderDigOre : OrderDig
{
  public OrderDigOre(Entity robot, (int, int) pos) : base(robot, pos)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    var cell = cx.GetCell(Pos);
    return Robot.Item == ItemType.Ore
           || !cell.Ore.HasValue && cell.Hole
           || cell.Ore.HasValue && cell.Ore == 0
           || !cell.IsSafe();
  }
}

public class OrderDigNearest : OrderDig
{
  private int _wasCloseAt = int.MinValue;
  public OrderDigNearest(Entity robot, (int, int) pos) : base(robot, pos)
  {
  }

  public override bool IsCompleted(Context cx)
  {
    return cx.Tick > 1 && _wasCloseAt + 1 == cx.Tick;
  }

  public override string ProduceCommand(Context cx)
  {
    var cell = cx.GetCell(Pos);

    if (!cell.IsSafe())
    {
      cell = cx.FindNearestSafe(Pos);
      if (cell == null)
      {
        Robot.Message = "no nearest";
        return null;
      }

      Pos = cell.Pos;
    }

    var distance = cell.Distance(Robot.Pos);
    if (distance == 1)
      _wasCloseAt = cx.Tick;

    return base.ProduceCommand(cx);
  }
}

public class OrderDigNearestRadar : OrderDigNearest
{
  private readonly int _radarId;

  public OrderDigNearestRadar(Entity robot, (int, int) pos, int radarId) : base(robot, pos)
  {
    _radarId = radarId;
  }

  public override void Finalize(Context cx)
  {
    base.Finalize(cx);
    if (Robot.Item != ItemType.Radar)
      Player.HighOrderScout.MyRadars.Add((Pos, _radarId));
  }
}

public class OrderTake : EOrder
{
  private readonly ItemType _item;

  public OrderTake(Entity robot, ItemType item) : base(robot)
  {
    _item = item;
  }

  public override bool IsCompleted(Context cx)
  {
    return Robot.Item == _item;
  }

  public override string ProduceCommand(Context cx)
  {
    return Entity.Take(_item);
  }
}

public class Utils
{
  public static IEnumerable<(int, int)> EnumerateNeighbors((int, int) from, int shift = 1)
  {
    var (x, y) = @from;
    if (x + shift < 30)
      yield return (x + shift, y);
    //yield return (x, y);
    if (y+shift < 15)
      yield return (x, y+shift);
    if (y-shift > 0)
      yield return (x, y-shift);
    yield return (x-shift, y);
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