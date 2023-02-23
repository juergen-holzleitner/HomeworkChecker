namespace HomeworkCheckerLib
{
  internal class OutputGenerator
  {
    private readonly IAppExecuter appExecuter;
    private readonly FilesystemService.IFileEnumerator fileEnumerator;

    internal record Output(int ExitCode, string Content, bool HasTimedOut);

    public OutputGenerator(FilesystemService.IFileEnumerator fileEnumerator, IAppExecuter appExecuter)
    {
      this.fileEnumerator = fileEnumerator;
      this.appExecuter = appExecuter;
    }

    internal Output GenerateOutput(string fileName, string? input)
    {
      string workingDirectory = Path.GetDirectoryName(fileName)!;

      const int timeoutInMilliseconds = 5000;
      var classFileName = Path.GetFileNameWithoutExtension(fileName);
      var result = appExecuter.Execute("java", workingDirectory, classFileName, input, timeoutInMilliseconds);
      return new(result.ExitCode, result.Output, result.HasTimedOut);
    }
    internal Output GenerateOutput(string fileName)
    {
      return GenerateOutput(fileName, null);
    }

    internal bool IsRunnableViaMain(string javaFile)
    {
      var fileContent = fileEnumerator.ReadFileContent(javaFile);
      return fileContent.Contains("main");
    }
  }
}
