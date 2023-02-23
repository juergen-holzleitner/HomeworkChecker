namespace HomeworkCheckerLib
{
  internal class JavaCompiler
  {
    private readonly IAppExecuter appExecuter;

    public JavaCompiler(IAppExecuter appExecuter)
    {
      this.appExecuter = appExecuter;
    }

    internal CompileResult CompileFile(IEnumerable<string> javaFilePaths)
    {
      var workingDirectory = Path.GetDirectoryName(javaFilePaths.First());
      var javaFiles = javaFilePaths.Select(f => string.Format($"\"{Path.GetFileName(f)}\""));

      var filesString = string.Join(' ', javaFiles);
      var executeResult = appExecuter.Execute("javac", workingDirectory!, $"-g -Xlint -encoding UTF8 {filesString}");

      return new(executeResult.ExitCode == 0, executeResult.Output);
    }

    internal record CompileResult(bool CompileSucceeded, string CompileOutput);

  }
}
