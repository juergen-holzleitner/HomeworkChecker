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
      return new(result.ExitCode, result.Output);
    }
  }
}
