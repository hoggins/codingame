using System.Collections.Generic;
using System.Linq;

public class Context
{
  public int Tick;
  public readonly Map Map  = new Map();

  public readonly List<Pac> Pacs = new List<Pac>();

  public void ReadInit()
  {
    Map.ReadInit();
  }

  public void ReadTick()
  {
    ++Tick;
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
      toRemove.Remove(pac);
    }

    foreach (var pac in toRemove)
      Pacs.Remove(pac);

    Map.ReadTick();

    foreach (var pac in Pacs)
      Map.SetPac(pac);
  }
}