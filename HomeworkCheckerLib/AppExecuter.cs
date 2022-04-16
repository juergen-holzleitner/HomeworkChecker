using System.Diagnostics;
using System.Text;

namespace HomeworkCheckerLib
{
  internal class AppExecuter : IAppExecuter
  {
    public IAppExecuter.ExecutionResult Execute(string appName, string workingDirectory, string arguments, string? input, int? timeout)
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
          output.AppendLine(args.Data);
      };

      process.StartInfo.RedirectStandardError = true;
      process.ErrorDataReceived += (sender, args) =>
      {
        if (args.Data is not null)
          output.AppendLine(args.Data);
      };

      process.StartInfo.RedirectStandardInput = true;

      process.Start();

      var inputStream = process.StandardInput;

      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      if (input != null)
        inputStream.Write(input);

      inputStream.Close();

      bool hasTimedOut = false;
      if (timeout.HasValue)
      {
        if (!process.WaitForExit(timeout.Value))
          hasTimedOut = true;
      }
      else
        process.WaitForExit();

      return new(process.ExitCode, output.ToString(), hasTimedOut);
    }

    public IAppExecuter.ExecutionResult Execute(string appName, string workingDirectory, string arguments)
    {
      return Execute(appName, workingDirectory, arguments, null, null);
    }
  }
}
