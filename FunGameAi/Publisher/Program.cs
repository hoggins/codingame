using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Publisher
{
  internal class Program
  {
    private static readonly Regex PatternAntyUsing = new Regex("^[\t ]*using (\\(|var).*");
    private static readonly Regex PatternUsing = new Regex("^[\t ]*using .*");
    private static readonly Regex PatternDefine = new Regex("^#define .*");

    public static void Main(string[] args)
    {
      var srcDir = string.Join(" ", args);
      var outFile = "theGame.cs";


      var usings = new HashSet<string>();
      var defines = new HashSet<string>();
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
            if (!PatternAntyUsing.IsMatch(line) && PatternUsing.IsMatch(line))
              usings.Add(line.Trim());
            else if (PatternDefine.IsMatch(line))
              defines.Add(line.Trim());
            else
              code.Add(line);
          }
        }
      }

      var mode = File.Exists(outFile) ? FileMode.Truncate : FileMode.OpenOrCreate;
      using (var fs = new FileStream(outFile, mode, FileAccess.Write))
      using (var sw = new StreamWriter(fs))
      {
        sw.WriteLine("#define PUBLISHED");
        foreach (var line in defines.OrderBy(v=>v))
          sw.WriteLine(line);

        foreach (var line in usings.OrderBy(v=>v))
          sw.WriteLine(line);

        foreach (var line in code)
        {
          sw.WriteLine(line);
        }
      }
    }
  }
}