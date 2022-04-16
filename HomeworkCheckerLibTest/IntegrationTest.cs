using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.Collections.Generic;
using System.IO;
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
      appExecuterMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));
      appExecuterMock.Setup(x => x.Execute("java", masterFolder, "someFile", "1\n2\n3\n", 5000))
                     .Returns(new IAppExecuter.ExecutionResult(0, "1", false));
      appExecuterMock.Setup(x => x.Execute("java", masterFolder, "someFile", "1\n1\n1\n", 5000))
                     .Returns(new IAppExecuter.ExecutionResult(-1, "1", true));
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "checkstyle"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "checkstyle issues", false));
      appExecuterMock.Setup(x => x.Execute("cmd.exe", Path.Combine("currentFolder", "pmd", "bin"), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "PMD issues", true));

      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(masterFolder, "*.java"))
              .Returns(new List<string> { @$"{masterFolder}\someFile.java" });

      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(masterFolder, "*.txt"))
              .Returns(new List<string> { @$"{masterFolder}\0.txt", @$"{masterFolder}\1.txt" });

      fileEnumeratorMock.Setup(f => f.ReadFileContent(@$"{masterFolder}\0.txt")).Returns("1\n2\n3\n");
      fileEnumeratorMock.Setup(f => f.ReadFileContent(@$"{masterFolder}\1.txt")).Returns("1\n1\n1\n");

      var outputMock = new Mock<IRuntimeOutput>(MockBehavior.Strict);
      var outputSequence = new MockSequence();
      outputMock.InSequence(outputSequence).Setup(o => o.WriteInfo("processing arbitraryFolder"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteSuccess("compiled someFile.java"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteSuccess("generated output for 0"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteError("generation of output for 1 timed out"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteWarning("checkstyle issues"));
      outputMock.InSequence(outputSequence).Setup(o => o.WriteWarning("PMD issues"));

      var sut = new HomeworkChecker(fileEnumeratorMock.Object, appExecuterMock.Object, outputMock.Object);

      var result = sut.ProcessMaster(masterFolder);

      var expectedMasterFile = Path.Combine(@"arbitraryFolder", @"someFile.java");
      result.MasterFile.Should().Be(Path.Combine(expectedMasterFile));
      result.CompileIssues.Should().BeEmpty();

      result.Outputs.Should().Equal(new List<HomeworkChecker.Output>
      {
        new(new HomeworkChecker.Input("0", "1\n2\n3\n"), "1", false),
        new(new HomeworkChecker.Input("1", "1\n1\n1\n"), "1", true)
      });

      result.CheckstyleIssues.Should().Be("checkstyle issues");

      outputMock.VerifyAll();
      outputMock.Verify(o => o.WriteWarning("PMD issues"), Times.Once());
    }

    [Fact]
    public void Print_error_if_compilation_failed()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(-1, string.Empty, false));
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");

      var outputMock = new Mock<IRuntimeOutput>();
      const string masterFolder = @"arbitraryFolder";
      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(masterFolder, "*.java"))
              .Returns(new List<string> { @$"{masterFolder}\someFile.java" });
      var sut = new HomeworkChecker(fileEnumeratorMock.Object, appExecuterMock.Object, outputMock.Object);

      var result = sut.ProcessMaster("arbitraryFolder");

      outputMock.Verify(o => o.WriteError("compiling someFile.java failed"), Times.Once());
      result.Outputs.Should().BeEmpty();

    }
  }
}