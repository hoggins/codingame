using System;
using System.Collections.Generic;
using System.IO;

namespace APackmanDebug
{
  class CGOutputProcessor
  {
    public List<string> States;

    public CGOutputProcessor(string fileName)
    {
      var data = File.ReadAllLines(fileName);

      States = new List<string>();
      var isOpened = false;
      for (var i = 0; i < data.Length; i++)
      {
        var line = data[i];
        if (line.Equals("Standard Error Stream:", StringComparison.InvariantCultureIgnoreCase))
        {
          if (!isOpened)
            States.Add(data[++i]);
          else
            States[States.Count - 1] += data[++i];

          isOpened = true;
        }
        else if (isOpened && line.StartsWith("done:", StringComparison.InvariantCultureIgnoreCase))
        {
          isOpened = false;
        }
      }
    }
  }
}