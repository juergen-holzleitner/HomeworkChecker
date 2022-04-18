using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class IntegrationTest
  {
    [Fact]
    public void Can_process_Master_folder()
    {
      const string masterFolder = @"arbitraryFolder";

      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute("javac", It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.Execute("java", masterFolder, "someFile", "1\n2\n3\n", 5000))
                     .Returns(new IAppExecuter.ExecutionResult(0, "1", false));
      appExecuterMock.Setup(x => x.Execute("java", masterFolder, "someFile", "1\n1\n1\n", 5000))
                     .Returns(new IAppExecuter.ExecutionResult(-1, "1", true));
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "checkstyle"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "checkstyle issues", false));
      appExecuterMock.Setup(x => x.Execute("cmd.exe", Path.Combine("currentFolder", "pmd", "bin"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "PMD content", true));
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "spotbugs", "lib"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "SpotBugs content", true));

      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(masterFolder, "*.java"))
              .Returns(new List<string> { @$"{masterFolder}\someFile.java" });

      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(masterFolder, "*.txt"))
              .Returns(new List<string> { @$"{masterFolder}\0.txt", @$"{masterFolder}\1.txt" });

      fileEnumeratorMock.Setup(f => f.ReadFileContent(@$"{masterFolder}\0.txt")).Returns("1\n2\n3\n");
      fileEnumeratorMock.Setup(f => f.ReadFileContent(@$"{masterFolder}\1.txt")).Returns("1\n1\n1\n");
      fileEnumeratorMock.Setup(f => f.ReadFileContent(@$"{masterFolder}\someFile.java")).Returns("printf");

      var outputMock = new Mock<IRuntimeOutput>(MockBehavior.Strict);
      var outputSequence = new MockSequence();
      outputMock.InSequence(outputSequence).Setup(o => o.WriteInfo("processing someFile.java"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteSuccess("compiled"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteSuccess("generated output for 0"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteError("generation of output for 1 timed out"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteWarning("custom analysis issues"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteWarning("checkstyle issues"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteWarning("PMD issues"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteWarning("SpotBugs issues"));

      var sut = new HomeworkChecker(fileEnumeratorMock.Object, appExecuterMock.Object, outputMock.Object);

      var result = sut.ProcessMaster(masterFolder);

      var expectedMasterFile = Path.Combine(@"arbitraryFolder", @"someFile.java");
      result.FileName.Should().Be(Path.Combine(expectedMasterFile));
      result.CompileIssues.Should().BeEmpty();

      result.Outputs.Should().Equal(new List<HomeworkChecker.Output>
      {
        new(new HomeworkChecker.Input("0", "1\n2\n3\n"), "1", false),
        new(new HomeworkChecker.Input("1", "1\n1\n1\n"), "1", true)
      });

      result.CheckstyleIssues.Should().Be("checkstyle issues");
      result.PMDIssues.Should().Be("PMD content");
      result.SpotBugsIssues.Should().Be("SpotBugs content");
      result.CustomAnalysisIssues.Should().NotBeEmpty();

      fileEnumeratorMock.Verify(f => f.RemoveFileIfExists(@$"{masterFolder}\someFile.class"));

      outputMock.VerifyAll();
    }

    [Fact]
    public void Can_process_homework_folder()
    {
      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly("masterFolder", "*.java"))
        .Returns(new List<string> { @$"masterFolder\HomeworkFile.java" });
      fileEnumeratorMock.Setup(f => f.ReadFileContent(@$"masterFolder\HomeworkFile.java")).Returns("master source");
      fileEnumeratorMock.Setup(f => f.ReadFileContent(It.Is<string>(s => s.StartsWith("homeworkFolder")))).Returns("homework source");
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly("homeworkFolder", "*.java"))
        .Returns(new List<string> { @$"homeworkFolder\homeworkFile.java", @$"homeworkFolder2\HomeworkFile.java" });

      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.Execute("javac", It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.Execute("java", "masterFolder", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "master output", false));
      appExecuterMock.Setup(x => x.Execute("java", It.Is<string>(s => s.StartsWith("homeworkFolder")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "homework output", false));
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "checkstyle"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.Execute("cmd.exe", Path.Combine("currentFolder", "pmd", "bin"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "spotbugs", "lib"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "jplag"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "Comparing \"masterFolder\\HomeworkFile.java\" - \"homeworkFolder\\homeworkFile.java\": 75\r\nComparing \"homeworkFolder2\\HomeworkFile.java\" - \"homeworkFolder\\homeworkFile.java\": 75", false));

      var outputMock = new Mock<IRuntimeOutput>();
      var sut = new HomeworkChecker(fileEnumeratorMock.Object, appExecuterMock.Object, outputMock.Object);

      var result = sut.ProcessHomework("masterFolder", "homeworkFolder");

      outputMock.Verify(o => o.WriteInfo(System.Environment.NewLine), Times.Exactly(3));
      outputMock.Verify(o => o.WriteWarning("processed jplag with 2 result(s), but 3 were expected"));
      outputMock.Verify(o => o.WriteInfo("processing homeworkFile.java"));
      outputMock.Verify(o => o.WriteInfo("processing HomeworkFile.java"));
      outputMock.Verify(o => o.WriteWarning("homeworkFolder\\homeworkFile.java has 1 duplicate(s)"));

      result.Submissions.Should().HaveCount(2);
      var homework1Submission = result.Submissions.Where(s => s.AnalysisResult.FileName == "homeworkFolder\\homeworkFile.java").Single();
      homework1Submission.Similarities.Duplicates.Should().Equal(new DuplicateFileAnalyzer.Similarity(@$"homeworkFolder2\HomeworkFile.java", DuplicateFileAnalyzer.SimilarityMode.ExactCopy));
      homework1Submission.Similarities.JplagSimilarities.Should().HaveCount(1);
      homework1Submission.Similarities.JplagMasterSimilarity.Should().NotBeNull();

      homework1Submission.FileNameAnalysis.FileNameDifference.Diffs.Should().HaveCount(3);

      homework1Submission.OutputDifference.Differences.Single().DifferenceType.Should().Be(OutputDifferencesAnalyzer.DifferenceType.Different);

    }
  }
}