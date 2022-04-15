namespace HomeworkCheckerLib
{
  internal class JavaCompiler
  {
    private readonly IAppExecuter appExecuter;

    public JavaCompiler(IAppExecuter appExecuter)
    {
      this.appExecuter = appExecuter;
    }

    internal object CompileFile(string javaFilePath)
    {
      var workingDirectory = Path.GetDirectoryName(javaFilePath);
      var javaFile = Path.GetFileName(javaFilePath);

      appExecuter.Execute("javac", workingDirectory!, $"-Xlint \"{javaFile}\"");
      return null;
    }
  }
}
