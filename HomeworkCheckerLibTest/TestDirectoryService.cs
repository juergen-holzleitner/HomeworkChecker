using FluentAssertions;
using HomeworkCheckerLib;
using System.Collections.Generic;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestDirectoryService
  {
    [Fact]
    public void Can_get_all_java_files_from_folders()
    {
      var folder = "arbitraryFolderName";
      var sut = new DirectoryService(new TestFileEnumerator());

      var files = sut.GetAllHomeWorkFolders(folder);

      files.Should().Equal(new List<string> { "folderA", "folderB" });
    }

    class TestFileEnumerator : DirectoryService.IFileEnumerator
    {
      public IEnumerable<string> GetFilesInFolderRecursivly(string folder, string extension)
      {
        extension.Should().Be("*.java");
        return new List<string> { @"folderA\aaa.java", @"folderB\bbb.java", @"folderB\bbb2.java" };
      }
    }
  }
}
