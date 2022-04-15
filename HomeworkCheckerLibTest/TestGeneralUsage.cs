using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestGeneralUsage
  {
    [Fact]
    public void Can_create_HomeworkChecker_to_process_Master_folder()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, string.Empty));

      const string masterFolder = @"arbitraryFolder";
      var fileEnumeratorMock = new Mock<DirectoryService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(masterFolder, "*.java"))
              .Returns(new List<string> { @$"{masterFolder}\someFile.java" });
      var sut = new HomeworkChecker(fileEnumeratorMock.Object, appExecuterMock.Object, Mock.Of<IRuntimeOutput>());

      var result = sut.ProcessMaster(masterFolder);

      var expectedMasterFile = Path.Combine(@"arbitraryFolder", @"someFile.java");
      result.MasterFile.Should().Be(Path.Combine(expectedMasterFile));
      result.CompileIssues.Issues.Should().BeEmpty();

      appExecuterMock.Verify(x => x.Execute("javac", "arbitraryFolder", "-Xlint \"someFile.java\""), Times.Once());
    }

    [Fact]
    public void Print_error_if_compilation_failed()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(new IAppExecuter.ExecutionResult(-1, string.Empty));
      var outputMock = new Mock<IRuntimeOutput>();
      const string masterFolder = @"arbitraryFolder";
      var fileEnumeratorMock = new Mock<DirectoryService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(masterFolder, "*.java"))
              .Returns(new List<string> { @$"{masterFolder}\someFile.java" });
      var sut = new HomeworkChecker(fileEnumeratorMock.Object, appExecuterMock.Object, outputMock.Object);

      sut.ProcessMaster("arbitraryFolder");

      outputMock.Verify(o => o.WriteError("compiling someFile.java failed"), Times.Once());
    }
  }
}