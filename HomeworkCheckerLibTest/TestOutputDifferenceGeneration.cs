using FluentAssertions;
using HomeworkCheckerLib;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestOutputDifferenceGeneration
  {
    [Fact]
    public void Can_generate_output_differences()
    {
      var input = new HomeworkChecker.Input("inputFile.txt", "input content");
      IEnumerable<HomeworkChecker.Output> masterOutput = new List<HomeworkChecker.Output> { new(input, "ouputcontent", false) };
      IEnumerable<HomeworkChecker.Output> submissionOutput = new List<HomeworkChecker.Output> { new(input, "ouput content", false) };

      var differenceAnalysis = OutputDifferencesAnalyzer.GetDifferences(masterOutput, submissionOutput);

      var diff = differenceAnalysis.Differences.Single();
      diff.MasterOutput.Should().Be(masterOutput.Single());
      diff.SubmissionOutput.Should().Be(submissionOutput.Single());
      diff.DifferenceType.Should().Be(OutputDifferencesAnalyzer.DifferenceType.WhitespaceOnly);
      diff.Difference.Diffs.Should().NotBeEmpty();
    }

    [Fact]
    public void Can_generate_output_differences_of_equal_files()
    {
      var input = new HomeworkChecker.Input("inputFile.txt", "input content");
      IEnumerable<HomeworkChecker.Output> masterOutput = new List<HomeworkChecker.Output> { new(input, "ouputcontent", false) };
      IEnumerable<HomeworkChecker.Output> submissionOutput = new List<HomeworkChecker.Output> { new(input, "ouputcontent", false) };

      var differenceAnalysis = OutputDifferencesAnalyzer.GetDifferences(masterOutput, submissionOutput);

      var diff = differenceAnalysis.Differences.Single();
      diff.MasterOutput.Should().Be(masterOutput.Single());
      diff.SubmissionOutput.Should().Be(submissionOutput.Single());
      diff.DifferenceType.Should().Be(OutputDifferencesAnalyzer.DifferenceType.Equal);
      diff.Difference.Diffs.Should().BeEmpty();
    }

    [Fact]
    public void Can_generate_output_differences_of_different_files()
    {
      var input = new HomeworkChecker.Input("inputFile.txt", "input content");
      IEnumerable<HomeworkChecker.Output> masterOutput = new List<HomeworkChecker.Output> { new(input, "ouputcontent", false) };
      IEnumerable<HomeworkChecker.Output> submissionOutput = new List<HomeworkChecker.Output> { new(input, "Xouputcontent", false) };

      var differenceAnalysis = OutputDifferencesAnalyzer.GetDifferences(masterOutput, submissionOutput);

      var diff = differenceAnalysis.Differences.Single();
      diff.MasterOutput.Should().Be(masterOutput.Single());
      diff.SubmissionOutput.Should().Be(submissionOutput.Single());
      diff.DifferenceType.Should().Be(OutputDifferencesAnalyzer.DifferenceType.Different);
      diff.Difference.Diffs.Should().HaveCount(2);
    }
  }
}
