namespace HomeworkChecker
{
  internal class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 1)
      {
        Console.WriteLine("USAGE: HomeworkChecker.exe <solution folder> [<homework folder>]");
        Console.WriteLine("USAGE: HomeworkChecker.exe --evaluateOnly <homework folder>");
        return;
      }

      if (args.Length == 1)
      {
        ProcessSolution(args[0]);
      }
      else
      {
        if (args[0] == "--evaluateOnly")
        {
          EvaluatePercentage(args[1]);
        }
        else
        {
          ProcessHomeworks(args[0], args[1]);
        }
      }
    }

    private static void ProcessHomeworks(string solutionFolder, string homeworkFolder)
    {
      var homeworkChecker = new HomeworkCheckerLib.HomeworkChecker();
      var analysisResult = homeworkChecker.ProcessHomework(solutionFolder, homeworkFolder);
      homeworkChecker.WriteAnalysisToMarkdownFile(analysisResult);

      if (homeworkChecker.StartVSCodeWithFolder(homeworkFolder) == 0)
      {
        homeworkChecker.CleanUpMarkdownFiles(analysisResult);
        EvaluatePercentage(analysisResult.SubmissionBaseFolder);
      }
    }

    private static void EvaluatePercentage(string homeworkFolder)
    {
      var percentageAdder = new HomeworkCheckerLib.PercentageAdder();
      percentageAdder.ProcessPercentages(homeworkFolder);
    }

    private static void ProcessSolution(string solutionFolder)
    {
      var homeworkChecker = new HomeworkCheckerLib.HomeworkChecker();
      var analysisResult = homeworkChecker.ProcessSolution(solutionFolder);
      homeworkChecker.WriteAnalysisToMarkdownFile(analysisResult);
      if (homeworkChecker.StartVSCodeWithFolder(solutionFolder) == 0)
        homeworkChecker.CleanUpMarkdownFiles(analysisResult);
    }
  }
}