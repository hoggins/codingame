using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Map
{
  public Cell[,] Layout;

  public void Init(int width, int height)
  {
    Layout = new Cell[width,height];
  }

  public Cell GetCell(int x, int y)
  {
    return Layout[x, y];
  }
}

struct Cell
{
  public byte? Ore;
  public bool HasHole;
}

enum EntityType
{
  None = -1,
  Robot = 0,
  RobotOpponent = 1,
  Radar = 2,
  Trap = 3,
  Ore = 4,
}

class Entity
{
  public int Id;
  public EntityType Type;
  public Point Pos;
  public EntityType Item;

  public Entity(int id)
  {
    Id = id;
  }
}

class GameState
{
  public int Tick;
  public int ScoreMy;
  public int ScoreEnemy;
  public int CooldownTrap;
  public int CooldownRadar;
  public readonly List<Entity> Entities = new List<Entity>();
  public readonly Map Map = new Map();

  public Entity GetOrAddEntity(int id)
  {
    var r = Entities.FirstOrDefault(x => x.Id == id);
    if (r == null)
      Entities.Add(r = new Entity(id));
    return r;
  }
}

class Program
{
  static void Main(string[] args)
  {
    string[] inputs;
    inputs = Console.ReadLine().Split(' ');
    int width = int.Parse(inputs[0]);
    int height = int.Parse(inputs[1]); // size of the map

    var gs = new GameState();
    gs.Map.Init(width, height);

    // game loop
    for (var tick = 0;; ++tick)
    {
      gs.Tick = tick;
      inputs = Console.ReadLine().Split(' ');
      gs.ScoreMy = int.Parse(inputs[0]); // Amount of ore delivered
      gs.ScoreEnemy = int.Parse(inputs[1]);
      for (var i = 0; i < height; i++)
      {
        inputs = Console.ReadLine().Split(' ');
        for (var j = 0; j < width; j++)
        {
          var cell = gs.Map.GetCell(width, height);
          string ore = inputs[2 * j]; // amount of ore or "?" if unknown
          cell.Ore = ore == "?" ? (byte?) null : byte.Parse(ore);
          cell.HasHole = inputs[2 * j + 1] == "1";
        }
      }

      inputs = Console.ReadLine().Split(' ');
      int entityCount = int.Parse(inputs[0]); // number of entities visible to you
      gs.CooldownRadar = int.Parse(inputs[1]); // turns left until a new radar can be requested
      gs.CooldownTrap = int.Parse(inputs[2]); // turns left until a new trap can be requested
      for (int i = 0; i < entityCount; i++)
      {
        inputs = Console.ReadLine().Split(' ');
        int entityId = int.Parse(inputs[0]); // unique id of the entity
        var entity = gs.GetOrAddEntity(entityId);
        entity.Type = (EntityType) int.Parse(inputs[1]); // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap
        int x = int.Parse(inputs[2]);
        int y = int.Parse(inputs[3]); // position of the entity
        entity.Pos = new Point(x,y);
        // if this entity is a robot, the item it is carrying (-1 for NONE, 2 for RADAR, 3 for TRAP, 4 for ORE)
        entity.Item = (EntityType) int.Parse(inputs[4]);
      }

      for (int i = 0; i < 5; i++)
      {
        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");

        Console.WriteLine("WAIT"); // WAIT|MOVE x y|DIG x y|REQUEST item
      }
    }
  }
}

abstract class Order
{
  public abstract double Evaluate(GameState gs, Entity robot);
}

class OrderScout : Order
{
  public override double Evaluate(GameState gs, Entity robot)
  {
throw new NotImplementedException();
  }
}

// class OrderDig : Order
// {
//
// }


