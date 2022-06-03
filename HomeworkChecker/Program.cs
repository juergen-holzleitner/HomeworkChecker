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

      var homeworkChecker = new HomeworkCheckerLib.HomeworkChecker();
      if (args.Length == 1)
      {
        var analysisResult = homeworkChecker.ProcessMaster(args[0]);
        homeworkChecker.WriteAnalysisToMarkdownFile(analysisResult);
        if (homeworkChecker.StartVSCodeWithFolder(args[0]) == 0)
          homeworkChecker.CleanUpMarkdownFiles(analysisResult);
      }
      else
      {
        if (args[0] == "--evaluateOnly")
        {
          var percentageAdder = new HomeworkCheckerLib.PercentageAdder();
          percentageAdder.ProcessPercentages(args[1]);
        }
        else
        {
          var analysisResult = homeworkChecker.ProcessHomework(args[0], args[1]);
          homeworkChecker.WriteAnalysisToMarkdownFile(analysisResult);

          if (homeworkChecker.StartVSCodeWithFolder(args[1]) == 0)
          {
            homeworkChecker.CleanUpMarkdownFiles(analysisResult);
            var percentageAdder = new HomeworkCheckerLib.PercentageAdder();
            percentageAdder.ProcessPercentages(analysisResult.SubmissionBaseFolder);
          }
        }
      }
    }
  }
}