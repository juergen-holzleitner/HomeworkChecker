using System.Diagnostics;

namespace HomeworkCheckerLib
{
  internal class CheckstyleProcessor
  {
    private readonly IAppExecuter appExecuter;

    public record CheckstyleResult(int ExitCode, string CheckstyleOutput);

    public CheckstyleProcessor(IAppExecuter appExecuter)
    {
      this.appExecuter = appExecuter;
    }

    internal CheckstyleResult Process(string javaPath)
    {
      var currentFolder = appExecuter.GetCurrentFolder();
      var result = appExecuter.Execute("java", Path.Combine(currentFolder, "checkstyle"), $"-jar \"checkstyle-10.3.3-all.jar\" -c google_checks_modified.xml \"{javaPath}\"");

      Debug.Assert(result.ExitCode == 0, $"checkstyle is not expected to return {result.ExitCode}");

      var output = CleanOutput(result.Output);
      var basePath = Path.GetDirectoryName(javaPath);
      if (!string.IsNullOrEmpty(basePath))
        output = RemovePathsFromOutput(output, basePath + Path.DirectorySeparatorChar);

      return new(result.ExitCode, output);
    }

    internal static string CleanOutput(string output)
    {
      output = output.Replace("Starting audit...\r\n", string.Empty);
      output = output.Replace("Audit done.\r\n", string.Empty);

      return output.Trim();
    }

    internal static string RemovePathsFromOutput(string output, string path)
    {
      output = output.Replace(path, string.Empty);

      return output;
    }
  }
}
