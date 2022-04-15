namespace HomeworkCheckerLib
{
  internal class OutputGenerator
  {
    private readonly IAppExecuter appExecuter;

    internal record Output(string Content);

    public OutputGenerator(IAppExecuter appExecuter)
    {
      this.appExecuter = appExecuter;
    }

    internal Output GenerateOutput(string fileName, string workingDirectory, string input)
    {
      var classFileName = Path.GetFileNameWithoutExtension(fileName);
      var result = appExecuter.Execute("java", workingDirectory, classFileName, input);
      return new(result.Output);
    }
  }
}
