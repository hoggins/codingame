using System;
using System.Collections.Generic;
#if !PUBLISHED
using System.IO;
#endif


class GameInput
{
  private readonly IEnumerator<string> _inputs;

  public GameInput()
  {
#if !PUBLISHED
    var inputs = File.ReadLines("Input.txt");
    _inputs = inputs.GetEnumerator();
#endif
  }

  public string ReadLine()
  {
#if !PUBLISHED
    _inputs.MoveNext();
    return _inputs.Current;
#endif

    var input = Console.ReadLine();
    //Player.Print(input);
    return input;
  }
}