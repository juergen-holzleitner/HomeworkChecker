namespace HomeworkCheckerLib
{
  internal interface IAppExecuter
  {
    record ExecutionResult(int ExitCode, string Output);

    ExecutionResult Execute(string appName, string workingDirectory, string arguments);
  }
}
