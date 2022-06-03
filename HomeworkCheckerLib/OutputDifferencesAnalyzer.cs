using System.Diagnostics;

namespace HomeworkCheckerLib
{
  public class OutputDifferencesAnalyzer
  {
    public enum DifferenceType { Equal, WhitespaceOnly, Different };
    public record OutputDifference(HomeworkChecker.Output SolutionOutput, HomeworkChecker.Output SubmissionOutput, DifferenceType DifferenceType, TextDiffGenerator.Difference Difference);

    public record OutputDifferenceAnalysis(IEnumerable<OutputDifference> Differences);

    internal static OutputDifferenceAnalysis GetDifferences(IEnumerable<HomeworkChecker.Output> solutionOutput, IEnumerable<HomeworkChecker.Output> submissionOutput)
    {
      Trace.Assert(solutionOutput.Count() == submissionOutput.Count());

      var differences = GetSubmissionDifferences(solutionOutput, submissionOutput);

      return new(differences);
    }

    private static IEnumerable<OutputDifference> GetSubmissionDifferences(IEnumerable<HomeworkChecker.Output> solutionOutput, IEnumerable<HomeworkChecker.Output> submissionOutput)
    {
      foreach (var s in submissionOutput)
      {
        var m = solutionOutput.Where(m => m.Input.Filename == s.Input.Filename).Single();
        yield return GetOutputDifference(m, s);
      }
    }

    private static OutputDifference GetOutputDifference(HomeworkChecker.Output solution, HomeworkChecker.Output submission)
    {
      var diff = TextDiffGenerator.GenerateDiff(submission.OutputContent, solution.OutputContent);

      if (solution.OutputContent == submission.OutputContent)
        return new(solution, submission, DifferenceType.Equal, diff);

      var diffType = TextDiffGenerator.GetStringWithoutWhiteSpaces(solution.OutputContent) == TextDiffGenerator.GetStringWithoutWhiteSpaces(submission.OutputContent) ? DifferenceType.WhitespaceOnly : DifferenceType.Different;

      return new(solution, submission, diffType, diff);
    }
  }
}
