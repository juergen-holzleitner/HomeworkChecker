using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeworkCheckerLib
{
  internal class SpotBugsProcessor
  {
    private readonly IAppExecuter appExecuter;

    public record SpotBugsResult(int ExitCode, string SpotBugsOutput);

    public SpotBugsProcessor(IAppExecuter appExecuter)
    {
      this.appExecuter = appExecuter;
    }

    internal SpotBugsResult Process(string javaPath)
    {
      var classFileName = Path.ChangeExtension(javaPath, "class");

      var currentFolder = appExecuter.GetCurrentFolder();
      var result = appExecuter.Execute("java", Path.Combine(currentFolder, "spotbugs", "lib"), $"-jar spotbugs.jar -textui -effort:max -low -longBugCodes -dontCombineWarnings -exclude ..\\exclude.xml \"{classFileName}\"");

      return new(result.ExitCode, result.Output);
    }
  }
}
