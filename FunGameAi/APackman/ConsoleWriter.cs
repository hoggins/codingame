using System.Linq;

class ConsoleWriter
{
  private readonly string _str;
  private int _i;
  public bool IsDone;

  public ConsoleWriter(string str)
  {
    _str = str;
    _i = 0;
  }

  public void Tick()
  {
    Player.Print("DumoKey:");
    Player.Print(new string(_str.Skip(_i*4000).Take(4000).ToArray()));
    ++_i;
    IsDone = _i * 4000 > _str.Length;

    if (IsDone)
      Player.Print("\n\ndone: " + _str.Length);
  }
}