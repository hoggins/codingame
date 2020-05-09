using System;

public enum PacType
{
  Rock, Paper, Scissors
}

public class Pac
{
  public int Id;
  public bool IsMine;
  public Point Pos;
  public PacType Type;
  public int SpeedTurnsLeft;
  public int AbilityCooldown;

  public int VisiblePellets;
  public POrderBase Order;

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

  public override string ToString()
  {
    return $"{Id} {Pos} {IsMine}";
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

  private PacType ParseType(string s)
  {
    switch (s)
    {
      case "ROCK": return PacType.Rock;
      case "PAPER": return PacType.Paper;
      case "SCISSORS": return PacType.Scissors;
      default: throw new Exception(s);
    }
  }
}

public class Rules
{
  public static bool CanBeat(PacType f, PacType t) => GetAdvantage(f) == t;

  public static PacType GetAdvantage(PacType f)
  {
    switch (f)
    {
      case PacType.Rock: return PacType.Scissors;
      case PacType.Paper: return  PacType.Rock;
      case PacType.Scissors: return PacType.Paper;
      default: throw new ArgumentOutOfRangeException();
    }
  }

  public static PacType GetVulnerability(PacType f)
  {
    switch (f)
    {
      case PacType.Rock: return PacType.Paper;
      case PacType.Paper: return PacType.Scissors;
      case PacType.Scissors: return PacType.Rock;
      default: throw new ArgumentOutOfRangeException();
    }
  }
}