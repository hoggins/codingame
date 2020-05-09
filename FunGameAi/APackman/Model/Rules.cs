using System;

public static class Rules
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