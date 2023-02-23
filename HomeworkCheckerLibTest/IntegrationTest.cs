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
    public void Can_enumerate_folders_with_code()
    {
      const string solutionFolder = @"arbitraryFolder";

      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(solutionFolder, "*.java"))
              .Returns(new List<string> 
              {
                @$"{solutionFolder}\FolderA\someFile.java",
                @$"{solutionFolder}\FolderB\someFile.java",
                @$"{solutionFolder}\FolderC\someFile.java" 
              });

      var sut = new HomeworkChecker(fileEnumeratorMock.Object, Mock.Of<IAppExecuter>(), Mock.Of<IRuntimeOutput>());

      var folders = sut.GetAllFoldersWithCode(solutionFolder);

      folders.Should().BeEquivalentTo(new[] { @$"{solutionFolder}\FolderA", @$"{solutionFolder}\FolderB", @$"{solutionFolder}\FolderC" });
    }

    [Fact]
    public void Can_process_Solution_folder()
    {
      const string solutionFolder = @"arbitraryFolder";

      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute("javac", It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.Execute("java", solutionFolder, "someFile", "1\n2\n3\n", 5000))
                     .Returns(new IAppExecuter.ExecutionResult(0, "1", false));
      appExecuterMock.Setup(x => x.Execute("java", solutionFolder, "someFile", "1\n1\n1\n", 5000))
                     .Returns(new IAppExecuter.ExecutionResult(0, "1", true));
      appExecuterMock.Setup(x => x.Execute("java", solutionFolder, "someFile", "3\n2\n1\n", 5000))
                     .Returns(new IAppExecuter.ExecutionResult(-1, "1", false));
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "checkstyle"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "checkstyle issues", false));
      appExecuterMock.Setup(x => x.Execute("cmd.exe", Path.Combine("currentFolder", "pmd", "bin"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "PMD content", true));
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "spotbugs", "lib"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "SpotBugs content", true));

      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(solutionFolder, "*.java"))
              .Returns(new List<string> { @$"{solutionFolder}\someFile.java" });

      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(solutionFolder, "*.txt"))
              .Returns(new List<string> { @$"{solutionFolder}\0.txt", @$"{solutionFolder}\1.txt", @$"{solutionFolder}\2.txt" });

      fileEnumeratorMock.Setup(f => f.ReadFileContent(@$"{solutionFolder}\0.txt")).Returns("1\n2\n3\n");
      fileEnumeratorMock.Setup(f => f.ReadFileContent(@$"{solutionFolder}\1.txt")).Returns("1\n1\n1\n");
      fileEnumeratorMock.Setup(f => f.ReadFileContent(@$"{solutionFolder}\2.txt")).Returns("3\n2\n1\n");
      fileEnumeratorMock.Setup(f => f.ReadFileContent(@$"{solutionFolder}\someFile.java")).Returns("main printf");

      var outputMock = new Mock<IRuntimeOutput>(MockBehavior.Strict);
      var outputSequence = new MockSequence();
      outputMock.InSequence(outputSequence).Setup(o => o.WriteInfo("processing arbitraryFolder"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteSuccess("compiled"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteSuccess("generated output for 0"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteError("generation of output for 1 timed out"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteError("generation of output for 2 has exit code -1"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteWarning("custom analysis issues"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteWarning("checkstyle issues"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteWarning("PMD issues"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteWarning("SpotBugs issues"));

      var sut = new HomeworkChecker(fileEnumeratorMock.Object, appExecuterMock.Object, outputMock.Object);

      var result = sut.ProcessSolution(solutionFolder);

      var expectedSolutionFile = Path.Combine(@"arbitraryFolder", @"someFile.java");
      result.FileNames.Should().BeEquivalentTo(new[] { expectedSolutionFile });
      result.CompileIssues.Should().BeEmpty();

      result.Outputs.Should().Equal(new List<HomeworkChecker.Output>
      {
        new(new HomeworkChecker.Input("0", "1\n2\n3\n"), 0, "1", false),
        new(new HomeworkChecker.Input("1", "1\n1\n1\n"), 0, "1", true),
        new(new HomeworkChecker.Input("2", "3\n2\n1\n"), -1, "1", false),
      });

      result.CheckstyleIssues.Should().Be("checkstyle issues");
      result.PMDIssues.Should().Be("PMD content");
      result.SpotBugsIssues.Should().Be("SpotBugs content");
      result.CustomAnalysisIssues.Should().NotBeEmpty();

      fileEnumeratorMock.Verify(f => f.RemoveFileIfExists(@$"{solutionFolder}\someFile.class"));

      outputMock.VerifyAll();
    }

    [Fact]
    public void Can_process_homework_folder()
    {
      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly("solutionFolder", "*.java"))
        .Returns(new List<string> { @"solutionFolder\HomeworkFile.java" });
      fileEnumeratorMock.Setup(f => f.ReadFileContent(@"solutionFolder\HomeworkFile.java")).Returns("solution source main");
      fileEnumeratorMock.Setup(f => f.ReadFileContent(It.Is<string>(s => s.StartsWith("homeworkFolder")))).Returns("homework source main");
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly("homeworkFolder", "*.java"))
        .Returns(new List<string> { @"homeworkFolder\homeworkFile.java", @"homeworkFolder2\HomeworkFile.java" });

      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.Execute("javac", It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.Execute("java", "solutionFolder", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "solution output", false));
      appExecuterMock.Setup(x => x.Execute("java", It.Is<string>(s => s.StartsWith("homeworkFolder")), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "homework output", false));
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "checkstyle"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.Execute("cmd.exe", Path.Combine("currentFolder", "pmd", "bin"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "spotbugs", "lib"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.ExecuteAsciiOutput("java", Path.Combine("currentFolder", "jplag"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "Comparing \"solutionFolder\\HomeworkFile.java\" - \"homeworkFolder\\homeworkFile.java\": 75\r\nComparing \"homeworkFolder2\\HomeworkFile.java\" - \"homeworkFolder\\homeworkFile.java\": 75", false));

      var outputMock = new Mock<IRuntimeOutput>();
      var sut = new HomeworkChecker(fileEnumeratorMock.Object, appExecuterMock.Object, outputMock.Object);

      var result = sut.ProcessHomework("solutionFolder", "homeworkFolder");

      outputMock.Verify(o => o.WriteInfo(System.Environment.NewLine), Times.Exactly(3));
      outputMock.Verify(o => o.WriteWarning("processed jplag with 2 result(s), but 3 were expected"));
      outputMock.Verify(o => o.WriteInfo("processing solutionFolder"));
      outputMock.Verify(o => o.WriteInfo("processing homeworkFolder"));
      outputMock.Verify(o => o.WriteWarning("homeworkFile.java has 1 duplicate(s)"));

      result.Submissions.Should().HaveCount(2);
      var homework1Submission = result.Submissions.Where(s => s.AnalysisResult.FileNames.Contains("homeworkFolder\\homeworkFile.java")).Single();
      homework1Submission.Similarities.Duplicates.Should().Equal(new DuplicateFileAnalyzer.Similarity(@"homeworkFolder2\HomeworkFile.java", DuplicateFileAnalyzer.SimilarityMode.ExactCopy));
      homework1Submission.Similarities.JplagSimilarities.Should().HaveCount(1);
      homework1Submission.Similarities.JplagSolutionSimilarity.Should().NotBeNull();

      homework1Submission.FileNameAnalysis.FileNameDifference.Diffs.Should().HaveCount(3);

      homework1Submission.OutputDifference.Differences.Single().DifferenceType.Should().Be(OutputDifferencesAnalyzer.DifferenceType.Different);
    }

    [Fact]
    public void Can_start_vscode_with_folder()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      var outputMock = new Mock<IRuntimeOutput>();
      var sut = new HomeworkChecker(Mock.Of<FilesystemService.IFileEnumerator>(), appExecuterMock.Object, outputMock.Object);

      appExecuterMock.Setup(x => x.Execute("cmd.exe", "folder", "/c code.cmd --wait .")).Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      int exitCode = sut.StartVSCodeWithFolder("folder");

      exitCode.Should().Be(0);
      outputMock.Verify(o => o.WriteInfo("waiting for VS code to close ..."));
    }

    [Fact]
    public void Can_cleanup_markdown_file()
    {
      var outputMock = new Mock<IRuntimeOutput>();
      var filesystemMock = new Mock<FilesystemService.IFileEnumerator>();
      var sut = new HomeworkChecker(filesystemMock.Object, Mock.Of<IAppExecuter>(), outputMock.Object);
      var fileAnalysisResult = new HomeworkChecker.FileAnalysisResult(new List<string> { "someFolder\\someFile.java" }, string.Empty, Enumerable.Empty<HomeworkChecker.Output>(), string.Empty, string.Empty, string.Empty, string.Empty);

      sut.CleanUpMarkdownFiles(fileAnalysisResult);

      filesystemMock.Verify(f => f.RemoveFileIfExists("someFolder\\NOTES.md"));

    }
  }
}