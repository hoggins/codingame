using System;

public enum PacType
{
  Rock = 0, Paper = 1, Scissors = 2, Dead = 3
}

[Serializable]
public class Pac
{
  public int Id;
  public bool IsMine;
  public Point Pos;
  public PacType Type;
  public int SpeedTurnsLeft;
  public int AbilityCooldown;

  public int VisiblePellets;

  public bool IsInClutch;

  [field:NonSerialized]
  public POrderBase Order { get; private set; }

  public bool IsBoosted => SpeedTurnsLeft > 0;
  public bool CanUseAbility => AbilityCooldown == 0;

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
    Type = ParseType(inputs[4]);
    SpeedTurnsLeft = int.Parse(inputs[5]);
    AbilityCooldown = int.Parse(inputs[6]);
  }

  public void SetOrder(Context cx, POrderBase order)
  {
    Order?.Complete(cx);
    Order = order;
    Order?.Start(cx);
  }

  public void Move(Point pellet, string msg = null)
  {
    Console.Write($"MOVE {Id >> 1} {pellet.X} {pellet.Y} {msg ?? string.Empty}|"); // MOVE <pacId> <x> <y>
  }

  public void Switch(PacType toType)
  {
    Console.Write($"SWITCH {Id >> 1} {toType.ToString().ToUpper()} |");
  }

  public void Boost()
  {
    Console.Write($"SPEED {Id >> 1} |");
  }

  public bool CanBeat(Pac pac) => Rules.CanBeat(Type, pac.Type);

  protected bool Equals(Pac other)
  {
    return Id == other.Id && IsMine == other.IsMine;
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != this.GetType()) return false;
    return Equals((Pac) obj);
  }

  public override int GetHashCode()
  {
    unchecked
    {
      return (Id * 397) ^ IsMine.GetHashCode();
    }
  }

  public override string ToString()
  {
    return $"{Id >> 1} m{IsMine} {Pos} pal{VisiblePellets}";
  }

  private PacType ParseType(string s)
  {
    switch (s)
    {
      case "ROCK": return PacType.Rock;
      case "PAPER": return PacType.Paper;
      case "SCISSORS": return PacType.Scissors;
      case "DEAD": return PacType.Dead;
      default: throw new Exception(s);
    }
  }
}