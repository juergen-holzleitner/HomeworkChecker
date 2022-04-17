﻿namespace HomeworkCheckerLib
{
  internal class OutputGenerator
  {
    private readonly IAppExecuter appExecuter;

    internal record Output(string Content, bool HasTimedOut);

    public OutputGenerator(IAppExecuter appExecuter)
    {
      this.appExecuter = appExecuter;
    }

    internal Output GenerateOutput(string fileName, string? input)
    {
      string workingDirectory = Path.GetDirectoryName(fileName)!;

      const int timeoutInMilliseconds = 5000;
      var classFileName = Path.GetFileNameWithoutExtension(fileName);
      var result = appExecuter.Execute("java", workingDirectory, classFileName, input, timeoutInMilliseconds);
      return new(result.Output, result.HasTimedOut);
    }
    internal Output GenerateOutput(string fileName)
    {
      return GenerateOutput(fileName, null);
    }
  }
}
