namespace HomeworkChecker
{
  internal class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 1)
      {
        Console.WriteLine("USAGE: HomeworkChecker.exe <master folder> [<homework folder>]");
        Console.WriteLine("USAGE: HomeworkChecker.exe --evaluateOnly <homework folder>");
        return;
      }

      if (args.Length == 1)
      {
        ProcessMaster(args[0]);
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

    private static void ProcessHomeworks(string masterFolder, string homeworkFolder)
    {
      var homeworkChecker = new HomeworkCheckerLib.HomeworkChecker();
      var analysisResult = homeworkChecker.ProcessHomework(masterFolder, homeworkFolder);
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

    private static void ProcessMaster(string masterFolder)
    {
      var homeworkChecker = new HomeworkCheckerLib.HomeworkChecker();
      var analysisResult = homeworkChecker.ProcessMaster(masterFolder);
      homeworkChecker.WriteAnalysisToMarkdownFile(analysisResult);
      if (homeworkChecker.StartVSCodeWithFolder(masterFolder) == 0)
        homeworkChecker.CleanUpMarkdownFiles(analysisResult);
    }
  }
}