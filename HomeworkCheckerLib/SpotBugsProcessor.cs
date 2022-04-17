using System.Diagnostics;

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

      Trace.Assert(result.ExitCode == 0, $"spotbugs is not expected to return {result.ExitCode}");

      return new(result.ExitCode, result.Output);
    }
  }
}
