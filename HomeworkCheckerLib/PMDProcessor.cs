﻿using System.Diagnostics;

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

    internal PMDResult Process(IEnumerable<string> javaFiles)
    {
      var directoryToAnalyze = Path.GetDirectoryName(javaFiles.First());

      var currentFolder = appExecuter.GetCurrentFolder();
      var result = appExecuter.Execute("cmd.exe", Path.Combine(currentFolder, "pmd", "bin"), $"/c pmd.bat -d \"{directoryToAnalyze}\" -R Rules.xml -f text --short-names --no-cache -language java");

      Debug.Assert(result.ExitCode == 0 || result.ExitCode == 4, $"PMD is not expected to return {result.ExitCode}");

      return new(result.ExitCode, result.Output);
    }
  }
}
