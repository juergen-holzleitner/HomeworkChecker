﻿namespace HomeworkChecker
{
  internal class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 1)
      {
        Console.WriteLine("USAGE: HomeworkChecker.exe <master folder> [<homework folder>] ");
        return;
      }

      var homeworkChecker = new HomeworkCheckerLib.HomeworkChecker();
      if (args.Length == 1)
      {
        var result = homeworkChecker.ProcessMaster(args[0]);
        Console.WriteLine(result.CheckstyleIssues);
      }
    }
  }
}