using System.Collections.Generic;
using System.Linq;

public class Context
{
  public int Tick;
  public readonly GameField Field  = new GameField();

  public int MyPacs;
  public readonly List<Pac> Pacs = new List<Pac>();

  public readonly InflMap Infl = new InflMap();

  public readonly SerialWriter Writer = new SerialWriter();

  public void ReadInit()
  {
    Field.ReadInit();
    Infl.Init(this);
  }

  public void ReadTick()
  {
    ++Tick;
    MyPacs = 0;
    var inputs = GameInput.ReadLine().Split(' ');
    var myScore = int.Parse(inputs[0]);
    var opponentScore = int.Parse(inputs[1]);
    var visiblePacCount = int.Parse(GameInput.ReadLine()); // all your pacs and enemy pacs in sight
    var toRemove = new List<Pac>(Pacs);
    for (var i = 0; i < visiblePacCount; i++)
    {
      inputs = GameInput.ReadLine().Split(' ');
      var pacId = int.Parse(inputs[0]); // pac number (unique within a team)
      var isMine = inputs[1] == "1";
      var trueId = (pacId << 1) | (isMine ? 1 : 0);

      var pac = Pacs.FirstOrDefault(p => p.Id == trueId);
      if (pac == null)
        Pacs.Add(pac = new Pac(trueId));

      pac.ReadTick(inputs);
      if (pac.Type != PacType.Dead)
        toRemove.Remove(pac);
      else
        toRemove.Add(pac);

      if (isMine)
        ++MyPacs;
    }

    foreach (var pac in toRemove)
      Pacs.Remove(pac);

    Field.ReadTick();

    foreach (var pac in Pacs)
      pac.VisiblePellets = Field.SetPac(pac);
  }

  public void UpdateTick()
  {
    Infl.TickUpdate(this);
  }
}