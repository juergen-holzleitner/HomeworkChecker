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
      const string folder = "arbitraryFolderName";

      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(folder, "*.java"))
              .Returns(new List<string> { @"folderA\aaa.java", @"folderB\bbb.java", @"folderB\bbb2.java" });
      var sut = new FilesystemService(fileEnumeratorMock.Object);

      var files = sut.GetAllHomeWorkFolders(folder);

      files.Should().Equal(new List<string> { "folderA", "folderB" });
    }

    [Fact]
    public void Can_get_all_input_files_from_folders()
    {
      const string folder = "arbitraryFolderName";

      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(
        f => f.GetFilesInFolderRecursivly(folder, "*.txt"))
              .Returns(new List<string> { "01.txt", "02.txt" });
      var sut = new FilesystemService(fileEnumeratorMock.Object);

      var files = sut.GetAllInputFiles(folder);

      files.Should().Equal(new List<string> { "01.txt", "02.txt" });
    }
  }
}
