namespace HomeworkCheckerLib
{
  internal class JavaCompiler
  {
    private readonly IAppExecuter appExecuter;

    public JavaCompiler(IAppExecuter appExecuter)
    {
      this.appExecuter = appExecuter;
    }

    internal CompileResult CompileFile(string javaFilePath)
    {
      var workingDirectory = Path.GetDirectoryName(javaFilePath);
      var javaFile = Path.GetFileName(javaFilePath);

      var executeResult = appExecuter.Execute("javac", workingDirectory!, $"-g -Xlint -encoding UTF8 \"{javaFile}\"");

      return new(executeResult.ExitCode == 0, executeResult.Output);
    }

    internal record CompileResult(bool CompileSucceeded, string CompileOutput);

  }
}
