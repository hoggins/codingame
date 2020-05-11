using System;
using System.Collections.Generic;
#if !PUBLISHED
using System.IO;

#endif

public class GameInput
{
  private static readonly IEnumerator<string> Inputs;

  static GameInput()
  {
#if !PUBLISHED
    var inputs = File.ReadLines("Input.txt");
    Inputs = inputs.GetEnumerator();
#endif
  }

  public static string ReadLine()
  {
#if !PUBLISHED
    Inputs.MoveNext();
    return Inputs.Current;
#endif

    var input = Console.ReadLine();
    //Console.Error.WriteLine(input);
    return input;
  }
}