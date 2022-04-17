using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeworkCheckerLib
{
  internal class JplagProcessor
  {
    private readonly IAppExecuter appExecuter;

    public record JplagResult(string JplagOutput);

    public JplagProcessor(IAppExecuter appExecuter)
    {
      this.appExecuter = appExecuter;
    }

    internal JplagResult Process(string masterFolder, string homeworkFolder)
    {
      const string jplagResultFolder = "TEMP-jplag-result";

      var currentFolder = appExecuter.GetCurrentFolder();
      var result = appExecuter.Execute("java", Path.Combine(currentFolder, "jplag"), $"-jar jplag-4.0.0-SNAPSHOT-jar-with-dependencies.jar -m 100 -t 4 -r \"{jplagResultFolder}\" \"{masterFolder}\" \"{homeworkFolder}\"");

      Trace.Assert(result.ExitCode == 0, $"jplag is not expected to return {result.ExitCode}");

      return new(result.Output);
    }
  }
}
