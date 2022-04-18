namespace HomeworkCheckerLib
{
  internal interface IAppExecuter
  {
    record ExecutionResult(int ExitCode, string Output, bool HasTimedOut);

    ExecutionResult Execute(string appName, string workingDirectory, string arguments);
    ExecutionResult ExecuteAsciiOutput(string appName, string workingDirectory, string arguments);
    ExecutionResult Execute(string appName, string workingDirectory, string arguments, string? input, int? timeout);
    string GetCurrentFolder();
  }
}
