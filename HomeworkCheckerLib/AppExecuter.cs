using System.Diagnostics;

namespace HomeworkCheckerLib
{
  internal class AppExecuter : IAppExecuter
  {
    public void Execute(string appName, string workingDirectory, string arguments)
    {
      using var process = new Process();
      process.StartInfo.FileName = appName;
      process.StartInfo.WorkingDirectory = workingDirectory;
      process.StartInfo.Arguments = arguments;

      process.Start();
      process.WaitForExit();
    }
  }
}
