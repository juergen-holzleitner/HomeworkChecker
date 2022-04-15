namespace HomeworkCheckerLib
{
  internal class RuntimeOutput : IRuntimeOutput
  {
    public void WriteError(string message)
    {
      var currentColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine(message);
      Console.ForegroundColor = currentColor;
    }
  }
}
