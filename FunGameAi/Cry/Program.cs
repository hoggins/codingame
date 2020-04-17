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
  public struct MapCell
  {
    public int? Ore;
    public bool Hole;

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

    public bool IsDead => X == -1 && Y == -1;

    public EOrder Order;

    public void Read(string[] inputs)
    {
      Id = int.Parse(inputs[0]); // unique id of the entity
      Type = (EntityType) int.Parse(inputs[1]); // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap
      X = int.Parse(inputs[2]);
      Y = int.Parse(inputs[3]); // position of the entity
      Item = (ItemType) int.Parse(
        inputs[4]); // if this entity is a robot, the item it is carrying (-1 for NONE, 2 for RADAR, 3 for TRAP, 4 for ORE)
    }
  }

  public class Context
  {
    public MapCell[,] Map;
    public List<Entity> Entities = new List<Entity>();
    public int RadarCooldown;
  }



  static void Main(string[] args)
  {
    string[] inputs;
    inputs = Console.ReadLine().Split(' ');
    int width = int.Parse(inputs[0]);
    int height = int.Parse(inputs[1]); // size of the map

    var cx = new Context();
    cx.Map = new MapCell[width, height];

    // game loop
    while (true)
    {
      inputs = Console.ReadLine().Split(' ');
      int myScore = int.Parse(inputs[0]); // Amount of ore delivered
      int opponentScore = int.Parse(inputs[1]);
      InputReadMap(cx, height, width);

      inputs = Console.ReadLine().Split(' ');
      int entityCount = int.Parse(inputs[0]); // number of entities visible to you
      cx.RadarCooldown = int.Parse(inputs[1]); // turns left until a new radar can be requested
      int trapCooldown = int.Parse(inputs[2]); // turns left until a new trap can be requested
      for (int i = 0; i < entityCount; i++)
      {
        inputs = Console.ReadLine().Split(' ');
        var id = int.Parse(inputs[0]); // unique id of the entity
        var entity = cx.Entities.Find(e => e.Id == id);
        if (entity == null)
          cx.Entities.Add(entity = new Entity());
        entity.Read(inputs);
      }

      foreach (var robot in cx.Entities)
      {
        if (robot.Type != EntityType.Robot)
          continue;

        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");
        //var robot = cx.Entities.Find(e => e.Id == i);
        if (robot.IsDead)
        {
          Console.WriteLine("WAIT");
          continue;
        }

        if (robot.Order == null || robot.Order.IsCompleted(cx, robot))
        {
          EOrder newOrder;
          var hasOrder = OrderHuntOre.TryProduce(cx, robot, out newOrder)
                         || OrderReturnOre.TryProduce(robot, out newOrder)
                         || (!cx.Entities.Any(e => e.Order is OrderPlaceRadar) &&
                             OrderPlaceRadar.TryProduce(cx, robot, out newOrder))
            ;
          robot.Order = newOrder;
        }

        var command = robot.Order?.ProduceCommand(cx) ?? "WAIT";
        // WAIT|MOVE x y|DIG x y|REQUEST item
        Console.WriteLine(command);
      }
    }
  }

  public static string Move((int, int Y) target)
  {
    return $"MOVE {target.Item1} {target.Item2}";
  }

  private class OrderPlaceRadar : EOrder
  {
    private readonly Entity _robot;
    private readonly MapCell[,] _map;
    private bool _isRequested;

    private OrderPlaceRadar(Entity robot)
    {
      _robot = robot;
    }

    public static bool TryProduce(Context cx, Entity robot, out EOrder newOrder)
    {
      newOrder = null;
      if (robot.Item != ItemType.None)
        return false;
      if (cx.RadarCooldown > 0)
        return false;
      newOrder = new OrderPlaceRadar(robot);
      return true;
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
        return Move((0, _robot.Y));
      }

      var iter = cx.Entities.Count(e => e.Type == EntityType.Radar) % 4;
      var posX = 7 + 7 * iter;
      var posY = 7 * (iter / 4 + 1);
      if (_robot.X == posX && _robot.Y == posY)
        return $"DIG {posX} {posY}";
      return Move((posX, posY));
    }
  }


  private class OrderReturnOre : EOrder
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
      return Move(Target);
    }
  }

  private class OrderHuntOre : EOrder
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
      var oreCell = FindOreCell(cx);
      if (!oreCell.HasValue)
        return false;
      order = new OrderHuntOre(oreCell.Value);
      return true;
    }

    public override bool IsCompleted(Context cx, Entity robot)
    {
      if (robot.Item != ItemType.None)
        return true;
      if (cx.Map[Target.Item1, Target.Item2].Ore < 1)
        return true;
      return false;
    }

    public override string ProduceCommand(Context cx)
    {
      return $"DIG {Target.Item1} {Target.Item2}";
    }
  }

  private static (int, int)? FindOreCell(Context cx)
  {
    for (int i = 0; i < cx.Map.GetLength(0); i++)
    {
      for (int j = 0; j < cx.Map.GetLength(1); j++)
      {
        if (cx.Map[i, j].Ore > 0)
          return (i, j);
      }
    }
    return null;
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
}