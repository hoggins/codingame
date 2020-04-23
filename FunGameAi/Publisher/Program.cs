﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Publisher
{
  internal class Program
  {
    private static readonly Regex PatternUsing = new Regex("^[\t ]*using .*");

    public static void Main(string[] args)
    {
      var srcDir = "../../../PlatinumRift2";
      var outFile = "theGame.cs";


      var usings = new HashSet<string>();
      var code = new List<string>();

      foreach (var fName in Directory.EnumerateFiles(srcDir, "*cs", SearchOption.AllDirectories))
      {
        if (Path.GetFileName(fName) == "AssemblyInfo.cs")
          continue;

        using (var fs = new FileStream(fName, FileMode.Open, FileAccess.Read))
        using(var sr = new StreamReader(fs))
        {
          while (true)
          {
            var line = sr.ReadLine();
            if (line == null)
              break;
            if (PatternUsing.IsMatch(line))
              usings.Add(line.Trim());
            else
              code.Add(line);
          }
        }
      }

      var mode = File.Exists(outFile) ? FileMode.Truncate : FileMode.OpenOrCreate;
      using (var fs = new FileStream(outFile, mode, FileAccess.Write))
      using (var sw = new StreamWriter(fs))
      {
        foreach (var line in usings.OrderBy(v=>v))
        {
          sw.WriteLine(line);
        }

        foreach (var line in code)
        {
          sw.WriteLine(line);
        }
      }
    }
  }
}