using System.Diagnostics;
using System.Text;

namespace HomeworkCheckerLib
{
  internal class AppExecuter : IAppExecuter
  {
    public IAppExecuter.ExecutionResult Execute(string appName, string workingDirectory, string arguments)
    {
      using var process = new Process();
      process.StartInfo.FileName = appName;
      process.StartInfo.WorkingDirectory = workingDirectory;
      process.StartInfo.Arguments = arguments;

      process.StartInfo.RedirectStandardOutput = true;
      var output = new StringBuilder();

      process.OutputDataReceived += (sender, args) => 
      {
        if (args.Data is not null)
          output.Append(args.Data);
      };

      process.StartInfo.RedirectStandardError = true;
      process.ErrorDataReceived += (sender, args) =>
      {
        if (args.Data is not null)
          output.Append(args.Data);
      };

      process.Start();

      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      process.WaitForExit();

      return new(process.ExitCode, output.ToString());
    }
  }
}
