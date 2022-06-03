using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.Collections.Generic;
using System.IO;
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
      IEnumerable<HomeworkChecker.Output> solutionOutput = new List<HomeworkChecker.Output> { new(input, 0, "ouputcontent", false) };
      IEnumerable<HomeworkChecker.Output> submissionOutput = new List<HomeworkChecker.Output> { new(input, 0, "ouput content", false) };

      var differenceAnalysis = OutputDifferencesAnalyzer.GetDifferences(solutionOutput, submissionOutput);

      var diff = differenceAnalysis.Differences.Single();
      diff.SolutionOutput.Should().Be(solutionOutput.Single());
      diff.SubmissionOutput.Should().Be(submissionOutput.Single());
      diff.DifferenceType.Should().Be(OutputDifferencesAnalyzer.DifferenceType.WhitespaceOnly);
      diff.Difference.Diffs.Should().NotBeEmpty();
    }

    [Fact]
    public void Can_generate_output_differences_of_equal_files()
    {
      var input = new HomeworkChecker.Input("inputFile.txt", "input content");
      IEnumerable<HomeworkChecker.Output> solutionOutput = new List<HomeworkChecker.Output> { new(input, 0, "ouputcontent", false) };
      IEnumerable<HomeworkChecker.Output> submissionOutput = new List<HomeworkChecker.Output> { new(input, 0, "ouputcontent", false) };

      var differenceAnalysis = OutputDifferencesAnalyzer.GetDifferences(solutionOutput, submissionOutput);

      var diff = differenceAnalysis.Differences.Single();
      diff.SolutionOutput.Should().Be(solutionOutput.Single());
      diff.SubmissionOutput.Should().Be(submissionOutput.Single());
      diff.DifferenceType.Should().Be(OutputDifferencesAnalyzer.DifferenceType.Equal);
      diff.Difference.Diffs.Should().BeEmpty();
    }

    [Fact]
    public void Can_generate_output_differences_of_different_files()
    {
      var input = new HomeworkChecker.Input("inputFile.txt", "input content");
      IEnumerable<HomeworkChecker.Output> solutionOutput = new List<HomeworkChecker.Output> { new(input, 0, "ouputcontent", false) };
      IEnumerable<HomeworkChecker.Output> submissionOutput = new List<HomeworkChecker.Output> { new(input, 0, "Xouputcontent", false) };

      var differenceAnalysis = OutputDifferencesAnalyzer.GetDifferences(solutionOutput, submissionOutput);

      var diff = differenceAnalysis.Differences.Single();
      diff.SolutionOutput.Should().Be(solutionOutput.Single());
      diff.SubmissionOutput.Should().Be(submissionOutput.Single());
      diff.DifferenceType.Should().Be(OutputDifferencesAnalyzer.DifferenceType.Different);
      diff.Difference.Diffs.Should().HaveCount(2);
      diff.Difference.Diffs[0].Should().Be(new DiffMatchPatch.Diff(DiffMatchPatch.Operation.DELETE, "X"));
      diff.Difference.Diffs[1].Should().Be(new DiffMatchPatch.Diff(DiffMatchPatch.Operation.EQUAL, "ouputcontent"));
    }

    [Fact]
    public void Do_not_call_output_generation_if_compile_failed()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute("javac", "solutionFolder", It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.Execute("javac", "homeworkFolder", It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(-1, "compile failed", false));
      appExecuterMock.Setup(x => x.Execute("java", "solutionFolder", "HomeworkFile", It.IsAny<string>(), It.IsAny<int>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "1", true));
      appExecuterMock.Setup(x => x.Execute("java", "homeworkFolder", "homeworkFile", It.IsAny<string>(), It.IsAny<int>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "1", true));
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "checkstyle"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "checkstyle issues", false));
      appExecuterMock.Setup(x => x.Execute("cmd.exe", Path.Combine("currentFolder", "pmd", "bin"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "PMD content", true));
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "spotbugs", "lib"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "SpotBugs content", true));
      appExecuterMock.Setup(x => x.ExecuteAsciiOutput("java", Path.Combine("currentFolder", "jplag"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "Comparing \"solutionFolder\\HomeworkFile.java\" - \"homeworkFolder\\homeworkFile.java\": 75\r\nComparing \"homeworkFolder2\\HomeworkFile.java\" - \"homeworkFolder\\homeworkFile.java\": 75", false));

      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly("solutionFolder", "*.java"))
        .Returns(new List<string> { @"solutionFolder\HomeworkFile.java" });
      fileEnumeratorMock.Setup(f => f.ReadFileContent(@"solutionFolder\HomeworkFile.java")).Returns("printf");
      fileEnumeratorMock.Setup(f => f.ReadFileContent(@"homeworkFolder\homeworkFile.java")).Returns("printf");
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly("homeworkFolder", "*.java"))
        .Returns(new List<string> { @"homeworkFolder\homeworkFile.java" });

      var outputMock = new Mock<IRuntimeOutput>();

      var sut = new HomeworkChecker(fileEnumeratorMock.Object, appExecuterMock.Object, outputMock.Object);

      var result = sut.ProcessHomework("solutionFolder", "homeworkFolder");

      outputMock.Verify(o => o.WriteError("compiling homeworkFile.java failed"));
      appExecuterMock.Verify(x => x.Execute("java", "homeworkFolder", "homeworkFile", It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }
  }
}
