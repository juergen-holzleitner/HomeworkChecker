using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeworkCheckerLib
{
  internal class JplagProcessor
  {
    private readonly IAppExecuter appExecuter;

    public record JplagResult(int ExitCode, string JplagOutput);

    public JplagProcessor(IAppExecuter appExecuter)
    {
      this.appExecuter = appExecuter;
    }

    internal JplagResult Process(string masterFolder, string homeworkFolder)
    {
      const string jplagResultFolder = "TEMP-jplag-result";

      var currentFolder = appExecuter.GetCurrentFolder();
      var result = appExecuter.Execute("java", Path.Combine(currentFolder, "jplag"), $"-jar jplag-4.0.0-SNAPSHOT-jar-with-dependencies.jar -m 100 -t 4 -r \"{jplagResultFolder}\" \"{masterFolder}\" \"{homeworkFolder}\"");

      return new(result.ExitCode, result.Output);
    }
  }
}
