using System;

public class Pac
{
  public int Id;
  public bool IsMine;
  public Point Pos;

  public POrderBase Order;

  public Pac(int id)
  {
    Id = id;
  }

  public void ReadTick(string[] inputs)
  {
    // Id = int.Parse(inputs[0]); // pac number (unique within a team)
    IsMine = inputs[1] == "1"; // true if this pac is yours
    var x = ushort.Parse(inputs[2]); // position in the grid
    var y = ushort.Parse(inputs[3]); // position in the grid
    Pos = new Point(x,y);
    var typeId = inputs[4]; // unused in wood leagues
    var speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
    var abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues
  }
  public override string ToString()
  {
    return $"{Id} {Pos} {IsMine}";
  }

  public void Move(Point pellet)
  {
    Console.Write($"MOVE {Id >> 1} {pellet.X} {pellet.Y} |"); // MOVE <pacId> <x> <y>
  }
}