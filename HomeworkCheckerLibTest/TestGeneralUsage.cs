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
      const string masterFolder = @"arbitraryFolder";
      var fileEnumeratorMock = Mock.Of<DirectoryService.IFileEnumerator>(
        l => l.GetFilesInFolderRecursivly(masterFolder, "*.java") == new List<string> { @"arbitraryFolder\someFile.java" }
        );
      var sut = new HomeworkChecker(fileEnumeratorMock);

      var result = sut.ProcessMaster(masterFolder);

      var expectedMasterFile = Path.Combine(@"arbitraryFolder", @"someFile.java");
      result.MasterFile.Should().Be(Path.Combine(expectedMasterFile));
    }
  }
}