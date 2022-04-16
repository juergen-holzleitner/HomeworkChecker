using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeworkCheckerLib
{
  internal class PMDProcessor
  {
    private readonly IAppExecuter appExecuter;

    public record PMDResult(int ExitCode, string PMDOutput);

    public PMDProcessor(IAppExecuter appExecuter)
    {
      this.appExecuter = appExecuter;
    }

    internal PMDResult Process(string javaPath)
    {
      var currentFolder = appExecuter.GetCurrentFolder();
      var result = appExecuter.Execute("cmd.exe", Path.Combine(currentFolder, "pmd", "bin"), $"/c pmd.bat -d \"{javaPath}\" -R Rules.xml -f text --short-names --no-cache -language java");
      
      return new(result.ExitCode, result.Output);
    }
  }
}
