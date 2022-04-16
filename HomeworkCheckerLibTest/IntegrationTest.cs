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

      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(masterFolder, "*.java"))
              .Returns(new List<string> { @$"{masterFolder}\someFile.java" });

      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(masterFolder, "*.txt"))
              .Returns(new List<string> { @$"{masterFolder}\0.txt" });

      fileEnumeratorMock.Setup(f => f.ReadFileContent(@$"{masterFolder}\0.txt")).Returns("1\n2\n3\n");

      var outputMock = new Mock<IRuntimeOutput>();

      var sut = new HomeworkChecker(fileEnumeratorMock.Object, appExecuterMock.Object, outputMock.Object);

      var result = sut.ProcessMaster(masterFolder);

      var expectedMasterFile = Path.Combine(@"arbitraryFolder", @"someFile.java");
      result.MasterFile.Should().Be(Path.Combine(expectedMasterFile));
      result.CompileIssues.Should().BeEmpty();

      appExecuterMock.Verify(x => x.Execute("javac", "arbitraryFolder", "-Xlint \"someFile.java\""), Times.Once());
      outputMock.Verify(o => o.WriteSuccess("compiled someFile.java"));

      result.Outputs.Should().Equal(new List<HomeworkChecker.Output> { new(new HomeworkChecker.Input("0", "1\n2\n3\n"), "1") });
    }

    [Fact]
    public void Print_error_if_compilation_failed()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(-1, string.Empty, false));
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