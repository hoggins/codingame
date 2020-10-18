using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

enum MoleculeId
{
  A = 0,
  B,
  C,
  D,
  E,
  N
}

static class MoleculeIdExt
{
  public static MoleculeId ParseMoleculeId(this string s)
  {
    if (s == "0")
      return MoleculeId.N;
    return (MoleculeId) Enum.Parse(typeof(MoleculeId), s);
  }
}

enum LocationId
{
  // ReSharper disable InconsistentNaming
  SAMPLES,
  DIAGNOSIS,
  MOLECULES,
  LABORATORY,
  START_POS,
  // ReSharper restore InconsistentNaming
}

struct Molecules
{
  public const int Count = 5;
  public sbyte A;
  public sbyte B;
  public sbyte C;
  public sbyte D;
  public sbyte E;


  public Molecules(int value)
  {
    A = (sbyte) value;
    B = (sbyte) value;
    C = (sbyte) value;
    D = (sbyte) value;
    E = (sbyte) value;
  }

  public static Molecules Parse(string[] arr, int idx)
  {
    return new Molecules
    {
      A = (sbyte) int.Parse(arr[idx + 0]),
      B = (sbyte) int.Parse(arr[idx + 1]),
      C = (sbyte) int.Parse(arr[idx + 2]),
      D = (sbyte) int.Parse(arr[idx + 3]),
      E = (sbyte) int.Parse(arr[idx + 4]),
    };
  }

  public int Total()
  {
    return A + B + C + D + E;
  }

  public bool CanSubtract(Molecules o)
  {
    return A >= o.A && B >= o.B && C >= o.C && D >= o.D && E >= o.E;
  }

  public Molecules Deficit(Molecules o)
  {
    return new Molecules
    {
      A = (sbyte) Math.Min(0, A - o.A),
      B = (sbyte) Math.Min(0, B - o.B),
      C = (sbyte) Math.Min(0, C - o.C),
      D = (sbyte) Math.Min(0, D - o.D),
      E = (sbyte) Math.Min(0, E - o.E),
    };
  }

  public void Add(MoleculeId moleculeId, sbyte toAdd)
  {
    switch (moleculeId)
    {
      case MoleculeId.A: A += toAdd; break;
      case MoleculeId.B: B += toAdd; break;
      case MoleculeId.C: C += toAdd; break;
      case MoleculeId.D: D += toAdd; break;
      case MoleculeId.E: E += toAdd; break;
      case MoleculeId.N: break;
      default:
        throw new ArgumentOutOfRangeException(nameof(moleculeId), moleculeId, null);
    }
  }

  public static Molecules operator +(Molecules a, Molecules b)
  {
    return new Molecules
    {
      A = (sbyte)(a.A + b.A),
      B = (sbyte)(a.B + b.B),
      C = (sbyte)(a.C + b.C),
      D = (sbyte)(a.D + b.D),
      E = (sbyte)(a.E + b.E),
    };
  }


  public static Molecules operator -(Molecules a, Molecules b)
  {
    return new Molecules
    {
      A = (sbyte)(a.A - b.A),
      B = (sbyte)(a.B - b.B),
      C = (sbyte)(a.C - b.C),
      D = (sbyte)(a.D - b.D),
      E = (sbyte)(a.E - b.E),
    };
  }

  public Molecules Abs()
  {
    return new Molecules
    {
      A = (sbyte)Math.Abs(A),
      B = (sbyte)Math.Abs(B),
      C = (sbyte)Math.Abs(C),
      D = (sbyte)Math.Abs(D),
      E = (sbyte)Math.Abs(E),
    };
  }

  public MoleculeId NameNotZero()
  {
    if (A != 0) return MoleculeId.A;
    if (B != 0) return MoleculeId.B;
    if (C != 0) return MoleculeId.C;
    if (D != 0) return MoleculeId.D;
    if (E != 0) return MoleculeId.E;
    return MoleculeId.N;
  }

  public override string ToString()
  {
    return $"a:{A} b:{B} c:{C} d:{D} e:{E}";
  }

  public MoleculeId NameHighest()
  {
    var maxV = A;
    var maxName = MoleculeId.A;
    if (B > maxV)
    {
      maxV = B;
      maxName = MoleculeId.B;
    }
    if (C > maxV)
    {
      maxV = C;
      maxName = MoleculeId.C;
    }
    if (D > maxV)
    {
      maxV = D;
      maxName = MoleculeId.D;
    }
    if (E > maxV)
    {
      maxV = E;
      maxName = MoleculeId.E;
    }

    return maxName;
  }
}

class Sample
{
  public int Id;
  public int CarriedBy;
  public int Rank;
  public MoleculeId ExpertiseGain;
  public int Health;
  public Molecules Cost;

  public bool IsFakeDiagnose;
  public bool IsDiagnosed => Cost.A >= 0;

  public static Sample Parse(string[] inputs)
  {
    return new Sample
    {
      Id = int.Parse(inputs[0]),
      CarriedBy = int.Parse(inputs[1]),
      Rank = int.Parse(inputs[2]),
      ExpertiseGain = inputs[3].ParseMoleculeId(),
      Health = int.Parse(inputs[4]),
      Cost = Molecules.Parse(inputs, 5),
    };
  }
}

class Robot
{
  public int Id;
  public LocationId Target;
  public int Eta;
  public int Score;
  public Molecules Storage;
  public Molecules Expertise;

  public static Robot Parse(int id, string[] inputs)
  {
    return new Robot
    {
      Id = id,
      Target = (LocationId) Enum.Parse(typeof(LocationId), inputs[0]),
      Eta = int.Parse(inputs[1]),
      Score = int.Parse(inputs[2]),
      Storage = Molecules.Parse(inputs, 3),
      Expertise = Molecules.Parse(inputs, 8),
    };
  }

  public IEnumerable<Sample> CarrySamples(GameState gs)
  {
    return gs.Samples.Where(x => x.CarriedBy == Id);
  }

  public IEnumerable<Sample> PrioritySamples(GameState gs)
  {
    return CarrySamples(gs).Where(x=>x.IsDiagnosed && !x.IsFakeDiagnose)
      .OrderBy(x =>
      {
        var toTake = (Storage + Expertise).Deficit(x.Cost).Abs();
        var isAvailable = gs.Available.CanSubtract(toTake);

        var basis = 0d;
        if (toTake.Total() == 0)
          basis = 0;
        else if (isAvailable && (Storage + toTake).Total() < 10)
          basis = 100;
        else if (isAvailable)
          basis = 150;
        else
          basis = 200;
        return basis + toTake.Total() * 1e3;
      });
  }
}

class GameState
{
  public List<Robot> Robots = new List<Robot>();
  public List<Sample> Samples = new List<Sample>();
  public Molecules Available;

  public Robot GetEnemy(int robotIdx)
  {
    return Robots[GameRules.GetEnemyIdx(robotIdx)];
  }
}

class Player
{
  static void Main(string[] args)
  {
    string[] inputs;
    int projectCount = int.Parse(Console.ReadLine());
    for (int i = 0; i < projectCount; i++)
    {
      var projects = Molecules.Parse(Console.ReadLine().Split(' '), 0);
    }

    // game loop
    while (true)
    {
      var gs = new GameState();

      for (int i = 0; i < 2; i++)
      {
        inputs = Console.ReadLine().Split(' ');
        gs.Robots.Add(Robot.Parse(i, inputs));
      }

      inputs = Console.ReadLine().Split(' ');
      gs.Available = Molecules.Parse(inputs, 0);
      int sampleCount = int.Parse(Console.ReadLine());
      for (int i = 0; i < sampleCount; i++)
      {
        inputs = Console.ReadLine().Split(' ');
        gs.Samples.Add(Sample.Parse(inputs));
      }

      // Write an action using Console.WriteLine()
      // To debug: Console.Error.WriteLine("Debug messages...");

      var r = gs.Robots[0];
      var robotBank = r.Storage + r.Expertise;
      foreach (var sample in r.PrioritySamples(gs))
      {
        if (robotBank.CanSubtract(sample.Cost))
        {
          Console.Error.WriteLine($"take {sample.Cost}");
          robotBank -= sample.Cost;
        }
        else
        {
          var deficit = robotBank.Deficit(sample.Cost);
          Console.Error.WriteLine($"deficit: {deficit}");
        }
      }

      var decision = Minimax.DecideMove(gs);
      if (decision == null)
        Console.WriteLine("WAIT");
      else
        decision.Execute(gs);
    }
  }
}

static class GameRules
{
  public static int GetMoveCost(LocationId from, LocationId to)
  {
    if (from == to)
      return 0;
    switch (from)
    {
      case LocationId.SAMPLES:
        return 3;
      case LocationId.DIAGNOSIS when to == LocationId.LABORATORY:
        return 4;
      case LocationId.DIAGNOSIS:
      case LocationId.MOLECULES:
        return 3;
      case LocationId.LABORATORY when to == LocationId.DIAGNOSIS:
        return 4;
      case LocationId.LABORATORY:
        return 3;
      default:
        return 2;
    }
  }

  public static int GetMinScore(int rank)
  {
    switch (rank)
    {
      case 3: return 20;
      case 2: return 10;
      default: return 1;
    }
  }

  public static int GetEnemyIdx(int robotIdx)
  {
    switch (robotIdx)
    {
      case 0: return 1;
      case 1: return 0;
      default:
        throw new ArgumentOutOfRangeException(nameof(robotIdx), robotIdx.ToString());
    }
  }
}

abstract class Decision
{
  public int RobotIdx;

  public abstract void Apply(GameState gs);
  public abstract void Undo(GameState gs);
  public abstract void Execute(GameState gs);

  protected Robot Owner(GameState gs) => gs.Robots[RobotIdx];
}

class DecisionGoTo : Decision
{
  private LocationId _startLoc;
  private LocationId _targetLoc;

  public DecisionGoTo(LocationId targetLoc)
  {
    _targetLoc = targetLoc;
  }

  public override void Apply(GameState gs)
  {
    var robot = Owner(gs);
    _startLoc = robot.Target;
    robot.Target = _targetLoc;
    robot.Eta = GameRules.GetMoveCost(_startLoc, _targetLoc);
  }

  public override void Undo(GameState gs)
  {
    var r = Owner(gs);
    r.Target = _startLoc;
    r.Eta = 0;
  }

  public override void Execute(GameState gs)
  {
    Console.WriteLine("GOTO " + _targetLoc);
  }

  public override string ToString()
  {
    return "go " + _targetLoc;
  }
}

class DecisionArrive : Decision
{
  public override void Apply(GameState gs)
  {
    var r = Owner(gs);
    --r.Eta;
  }

  public override void Undo(GameState gs)
  {
    var r = Owner(gs);
    ++r.Eta;
  }

  public override void Execute(GameState gs)
  {
    Console.WriteLine("WAIT");
  }

  public override string ToString()
  {
    return "arrive";
  }
}

class DecisionTakeSample : Decision
{
  private static int FakeSampleId = 5000;

  public int Rank = 2;

  private Sample _generated;

  public override void Apply(GameState gs)
  {
    var sample = new Sample
    {
      Id = ++FakeSampleId,
      CarriedBy = RobotIdx,
      Rank = Rank,
      ExpertiseGain = MoleculeId.N,
      Health = 1,
      Cost = new Molecules(-1),
    };
    _generated = sample;
    gs.Samples.Add(sample);
  }

  public override void Undo(GameState gs)
  {
    gs.Samples.Remove(_generated);
    _generated = null;
  }

  public override void Execute(GameState gs)
  {
    Console.WriteLine("CONNECT " + Rank);
  }

  public override string ToString()
  {
    return "take sample " + Rank;
  }
}

class DecisionDiagnose : Decision
{
  private Sample _sample;

  public DecisionDiagnose(Sample sample)
  {
    _sample = sample;
  }

  public override void Apply(GameState gs)
  {
    _sample.Cost = new Molecules(1);
    _sample.IsFakeDiagnose = true;
  }

  public override void Undo(GameState gs)
  {
    _sample.Cost = new Molecules(-1);
    _sample.IsFakeDiagnose = false;
  }

  public override void Execute(GameState gs)
  {
    Console.WriteLine("CONNECT " + _sample.Id);
  }

  public override string ToString()
  {
    return "diag";
  }
}

class DecisionTakeMolecule : Decision
{
  private MoleculeId _moleculeId;

  public DecisionTakeMolecule(MoleculeId moleculeId)
  {
    _moleculeId = moleculeId;
  }

  public override void Apply(GameState gs)
  {
    var r = Owner(gs);
    r.Storage.Add(_moleculeId, 1);
    gs.Available.Add(_moleculeId, -1);
  }

  public override void Undo(GameState gs)
  {
    var r = Owner(gs);
    r.Storage.Add(_moleculeId, -1);
    gs.Available.Add(_moleculeId, 1);
  }

  public override void Execute(GameState gs)
  {
    Console.WriteLine("CONNECT " + _moleculeId);
  }

  public override string ToString()
  {
    return "take " + _moleculeId;
  }
}

class DecisionComplete : Decision
{
  private Sample _sample;

  public DecisionComplete(Sample sample)
  {
    _sample = sample;
  }

  public override void Apply(GameState gs)
  {
    var r = Owner(gs);
    r.Score += _sample.Health;
    r.Expertise.Add(_sample.ExpertiseGain, 1);

    // todo consider last molecule taken
    gs.Available += _sample.Cost;

    gs.Samples.Remove(_sample);
  }

  public override void Undo(GameState gs)
  {
    var r = Owner(gs);
    r.Score -= _sample.Health;
    r.Expertise.Add(_sample.ExpertiseGain, -1);
    gs.Available -= _sample.Cost;
    gs.Samples.Add(_sample);
  }

  public override void Execute(GameState gs)
  {
    Console.WriteLine("CONNECT " + _sample.Id);
  }

  public override string ToString()
  {
    return "compl";
  }
}

static class Minimax
{
  class Variation
  {
    public double Score;
    public List<(Decision, Decision)> Decisions = new List<(Decision, Decision)>();

    public Variation(double score)
    {
      Score = score;
    }

    public void Push(Decision b0, Decision b1)
    {
      Decisions.Add((b0,b1));
    }

    public string PathString()
    {
      var sb = new StringBuilder();
      for (var i = Decisions.Count - 1; i >= 0; i--)
      {
        sb.Append($"{Decisions[i].Item1} -> ");
      }

      return sb.ToString();
    }
  }

  public static Decision DecideMove(GameState gs)
  {
    var variation = Simulate(gs, 0, 10, Double.NegativeInfinity, Double.PositiveInfinity);
    if (variation.Decisions.Count == 0)
      return null;


    Console.Error.WriteLine("selected: " + variation.PathString());


    return variation.Decisions.Last().Item1;
  }

  private static Variation Simulate(GameState gs, int depth, int maxDepth, double alpha, double beta)
  {
    if (depth == maxDepth)
      return new Variation(Heuristic.Evaluate(gs));

    var localAlpha = alpha;
    var branch0 = Heuristic.PossibleMoves(gs, 0);
    var branch1 = Heuristic.PossibleMoves(gs, 1);

    Variation alphaBest = null;

    foreach (var b0 in branch0)
    {
      var localBeta = beta;
      Variation betaBest = null;
      foreach (var b1 in branch1)
      {
        ApplyActions(gs, b0, b1);
        var variation = Simulate(gs, depth + 1, maxDepth, localAlpha, localBeta);
        UndoActions(gs, b0, b1);
        localBeta = Math.Min(localBeta, variation.Score);
        if (betaBest == null || variation.Score < betaBest.Score)
        {
          betaBest = variation;
          betaBest.Push(b0, b1);
        }
        if (localBeta <= localAlpha)
          break;
      }

      if (betaBest == null)
        continue;

      localAlpha = Math.Max(localAlpha, betaBest.Score);
      if (alphaBest == null || betaBest.Score > alphaBest.Score)
      {
        alphaBest = betaBest;
        //Console.Error.WriteLine($"dep {depth} na {alphaBest.Score:0.000} p:{alphaBest.PathString()}");
      }
      if (beta < alpha)
        break;
    }

    if (alphaBest != null)
      return alphaBest;

    return new Variation(Heuristic.Evaluate(gs));
  }

  private static void ApplyActions(GameState gs, Decision b0, Decision b1)
  {
    b0.Apply(gs);
    b1.Apply(gs);

    //Console.Error.WriteLine("-> " + b0);
  }

  private static void UndoActions(GameState gs, Decision b0, Decision b1)
  {
    b0.Undo(gs);
    b1.Undo(gs);

    //Console.Error.WriteLine("<- " + b0);
  }
}

static class Heuristic
{
  public static List<Decision> PossibleMoves(GameState gs, int robotIdx)
  {
    var robot = gs.Robots[robotIdx];
    var res = new List<Decision>();
    if (robot.Eta != 0)
      res.Add(new DecisionArrive());
    else
      switch (robot.Target)
      {
        case LocationId.SAMPLES:
          if (robot.CarrySamples(gs).Count() < 3)
            res.Add(new DecisionTakeSample());
          res.Add(new DecisionGoTo(LocationId.DIAGNOSIS));
          break;

        case LocationId.DIAGNOSIS:
          var undiagnosed = robot.CarrySamples(gs).FirstOrDefault(x => !x.IsDiagnosed);
          if (undiagnosed != null)
            res.Add(new DecisionDiagnose(undiagnosed));
          res.Add(new DecisionGoTo(LocationId.MOLECULES));
          res.Add(new DecisionGoTo(LocationId.LABORATORY));
          break;

        case LocationId.MOLECULES:
          res.Add(new DecisionGoTo(LocationId.LABORATORY));
          AddBestToTake(gs, robot, res);
          //AddBestToTake(gs, gs.GetEnemy(robotIdx), res);

          break;

        case LocationId.LABORATORY:
          var completed = robot.CarrySamples(gs)
            .FirstOrDefault(x =>
              x.IsDiagnosed
              && !x.IsFakeDiagnose
              && (robot.Storage + robot.Expertise).CanSubtract(x.Cost));
          if (completed != null)
          {
            res.Add(new DecisionComplete(completed));
          }

          res.Add(new DecisionGoTo(LocationId.SAMPLES));
          res.Add(new DecisionGoTo(LocationId.MOLECULES));
          break;
        case LocationId.START_POS:
          res.Add(new DecisionGoTo(LocationId.SAMPLES));
          break;

        default:
          throw new ArgumentOutOfRangeException();
      }

    foreach (var decision in res)
    {
      decision.RobotIdx = robotIdx;
    }

    return res;
  }

  private static void AddBestToTake(GameState gs, Robot robot, List<Decision> res)
  {
    if (robot.Storage.Total() < 10)
    {
      var robotBank = robot.Expertise + robot.Storage;
      foreach (var sample in robot.PrioritySamples(gs))
      {
        if (robotBank.CanSubtract(sample.Cost))
        {
          robotBank -= sample.Cost;
          continue;
        }

        var deficit = robotBank.Deficit(sample.Cost).Abs();
        if (!gs.Available.CanSubtract(deficit))
          continue;

        var moleculeId = deficit.NameHighest();
        res.Add(new DecisionTakeMolecule(moleculeId));
        break;
      }
    }
  }

  public static double Evaluate(GameState gs)
  {
    var p0 = EvaluatePlayer(gs, 0);
    var p1 = EvaluatePlayer(gs, 1);
    return p0 - p1;
  }

  private static double EvaluatePlayer(GameState gs, int robotIdx)
  {
    const double expertiseCoef = 10;
    var robot = gs.Robots[robotIdx];
    var score = 0d;
    score += robot.Score;
    score += robot.Expertise.Total() * expertiseCoef;

    var robotBank = robot.Expertise + robot.Storage;
    var fieldBank = robot.Expertise + robot.Storage + gs.Available;
    var sampleIdx = 1;

    var carrySamples = robot.CarrySamples(gs).ToArray();
    foreach (var sample in carrySamples.Where(x=>x.IsDiagnosed && !x.IsFakeDiagnose))
    {
      var isComplete = robotBank.CanSubtract(sample.Cost);
      var isProducible = fieldBank.CanSubtract(sample.Cost);
      var toTake = robotBank.Deficit(sample.Cost);
      if (isComplete)
      {
        score += 0.85 * (sample.Health + expertiseCoef);
        score -= 0.01 * (GameRules.GetMoveCost(robot.Target, LocationId.LABORATORY) + robot.Eta);
      }
      else if (isProducible && (robot.Storage + toTake).Total() < 10)
      {
        score += 0.5 * (sample.Health + expertiseCoef);
      }
      else
      {
        score += 0.05 * (sample.Health + expertiseCoef);
      }

      if (!isComplete)
        score -= 1e-2 * Math.Pow(0.5,sampleIdx) * toTake.Total();

      score -= 1e-2 / 4 * (10 - robot.Storage.Total());

      if (isComplete || isProducible)
      {
        robotBank -= sample.Cost;
        fieldBank -= sample.Cost;
      }
      ++sampleIdx;
    }

    foreach (var sample in carrySamples.Where(x => !x.IsDiagnosed || x.IsFakeDiagnose))
    {
      if (!sample.IsDiagnosed)
      {
        score += 0.15 * (GameRules.GetMinScore(sample.Rank) + expertiseCoef);
        score -= 0.01 * (GameRules.GetMoveCost(robot.Target, LocationId.DIAGNOSIS) + robot.Eta);
      }
      else
      {
        score += 0.175 * (GameRules.GetMinScore(sample.Rank) + expertiseCoef);
      }
    }

    score -= 0.01 * (3 - carrySamples.Length) * (GameRules.GetMoveCost(robot.Target, LocationId.SAMPLES) + robot.Eta);
    return score;
  }
}