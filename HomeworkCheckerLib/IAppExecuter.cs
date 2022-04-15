namespace HomeworkCheckerLib
{
  internal interface IAppExecuter
  {
    record ExecutionResult(int ExitCode);

    ExecutionResult Execute(string appName, string workingDirectory, string arguments);
  }
}
