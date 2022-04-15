using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public partial class TestDirectoryService
  {
    [Fact]
    public void Can_get_all_java_files_from_folders()
    {
      var fileEnumeratorMock = new Mock<DirectoryService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly("arbitraryFolderName", "*.java"))
              .Returns(new List<string> { @"folderA\aaa.java", @"folderB\bbb.java", @"folderB\bbb2.java" });
      var folder = "arbitraryFolderName";
      var sut = new DirectoryService(fileEnumeratorMock.Object);

      var files = sut.GetAllHomeWorkFolders(folder);

      files.Should().Equal(new List<string> { "folderA", "folderB" });
    }
  }
}
