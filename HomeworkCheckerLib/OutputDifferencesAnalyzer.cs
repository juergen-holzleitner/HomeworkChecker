using System.Diagnostics;

namespace HomeworkCheckerLib
{
  public class OutputDifferencesAnalyzer
  {
    public enum DifferenceType { Equal, WhitespaceOnly, Different };
    public record OutputDifference(HomeworkChecker.Output MasterOutput, HomeworkChecker.Output SubmissionOutput, DifferenceType DifferenceType, TextDiffGenerator.Difference Difference);

    public record OutputDifferenceAnalysis(IEnumerable<OutputDifference> Differences);

    internal static OutputDifferenceAnalysis GetDifferences(IEnumerable<HomeworkChecker.Output> masterOutput, IEnumerable<HomeworkChecker.Output> submissionOutput)
    {
      Trace.Assert(masterOutput.Count() == submissionOutput.Count());

      var differences = GetSubmissionDiffernces(masterOutput, submissionOutput);

      return new(differences);
    }

    private static IEnumerable<OutputDifference> GetSubmissionDiffernces(IEnumerable<HomeworkChecker.Output> masterOutput, IEnumerable<HomeworkChecker.Output> submissionOutput)
    {
      foreach (var s in submissionOutput)
      {
        var m = masterOutput.Where(m => m.Input.Filename == s.Input.Filename).Single();
        yield return GetOutputDifference(m, s);
      }
    }

    private static OutputDifference GetOutputDifference(HomeworkChecker.Output master, HomeworkChecker.Output submission)
    {
      var diff = TextDiffGenerator.GenerateDiff(master.OutputContent, submission.OutputContent);

      if (master.OutputContent == submission.OutputContent)
        return new(master, submission, DifferenceType.Equal, diff);

      var diffType = TextDiffGenerator.GetStringWithoutWhiteSpaces(master.OutputContent) == TextDiffGenerator.GetStringWithoutWhiteSpaces(submission.OutputContent) ? DifferenceType.WhitespaceOnly : DifferenceType.Different;

      return new(master, submission, diffType, diff);
    }
  }
}
