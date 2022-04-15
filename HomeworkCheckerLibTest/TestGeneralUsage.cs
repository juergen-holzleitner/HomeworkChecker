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
                     .Returns(new IAppExecuter.ExecutionResult(0));

      const string masterFolder = @"arbitraryFolder";
      var fileEnumeratorMock = new Mock<DirectoryService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(masterFolder, "*.java"))
              .Returns(new List<string> { @"arbitraryFolder\someFile.java" });
      var sut = new HomeworkChecker(fileEnumeratorMock.Object, appExecuterMock.Object);

      var result = sut.ProcessMaster(masterFolder);

      var expectedMasterFile = Path.Combine(@"arbitraryFolder", @"someFile.java");
      result.MasterFile.Should().Be(Path.Combine(expectedMasterFile));
      result.CompileIssues.Issues.Should().BeEmpty();

      appExecuterMock.Verify(x => x.Execute("javac", "arbitraryFolder", "-Xlint \"someFile.java\""), Times.Once());
    }
  }
}