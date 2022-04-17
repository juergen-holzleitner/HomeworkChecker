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
    private readonly FilesystemService.IFileEnumerator filesystemService;

    public record JplagResult(string JplagOutput);

    public JplagProcessor(IAppExecuter appExecuter, FilesystemService.IFileEnumerator filesystemService)
    {
      this.appExecuter = appExecuter;
      this.filesystemService = filesystemService;
    }

    internal JplagResult Process(string masterFolder, string homeworkFolder)
    {
      const string jplagResultFolder = "TEMP-jplag-result";

      var currentFolder = appExecuter.GetCurrentFolder();
      var workingFolder = Path.Combine(currentFolder, "jplag");
      var result = appExecuter.Execute("java", workingFolder, $"-jar jplag-4.0.0-SNAPSHOT-jar-with-dependencies.jar -m 100 -t 4 -r \"{jplagResultFolder}\" \"{masterFolder}\" \"{homeworkFolder}\"");

      Trace.Assert(result.ExitCode == 0, $"jplag is not expected to return {result.ExitCode}");

      filesystemService.RemoveFolderIfExists(Path.Combine(workingFolder, jplagResultFolder));
      return new(result.Output);
    }
  }
}
