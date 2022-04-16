using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      var result = appExecuter.Execute("java", Path.Combine(currentFolder, "checkstyle"), $"-jar \"checkstyle-10.1-all.jar\" -c google_checks_modified.xml \"{javaPath}\"");
      var output = CleanOutput(result.Output);
      return new(result.ExitCode, output);
    }

    internal static string CleanOutput(string output)
    {
      output = output.Replace("Starting audit...\r\n", string.Empty);
      output = output.Replace("Audit done.\r\n", string.Empty);

      return output.Trim();
    }
  }
}
