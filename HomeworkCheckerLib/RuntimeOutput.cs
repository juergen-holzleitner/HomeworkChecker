namespace HomeworkCheckerLib
{
  internal class RuntimeOutput : IRuntimeOutput
  {
    public void WriteError(string message)
    {
      WriteColoredOutput(message, ConsoleColor.Red);
    }
    public void WriteSuccess(string message)
    {
      WriteColoredOutput(message, ConsoleColor.Green);
    }

    private void WriteColoredOutput(string message, ConsoleColor color)
    {
      var currentColor = Console.ForegroundColor;
      Console.ForegroundColor = color;
      Console.WriteLine(message);
      Console.ForegroundColor = currentColor;
    }
  }
}
