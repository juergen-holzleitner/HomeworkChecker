using FluentAssertions;
using HomeworkCheckerLib;
using System.Collections.Generic;

namespace HomeworkCheckerLibTest
{
  class FileEnumeratorMock : DirectoryService.IFileEnumerator
  {
    private readonly IEnumerable<string> fileToReturn;

    public FileEnumeratorMock(IEnumerable<string> fileToReturn)
    {
      this.fileToReturn = fileToReturn;
    }

    public IEnumerable<string> GetFilesInFolderRecursivly(string folder, string extension)
    {
      extension.Should().Be("*.java");
      return fileToReturn;
    }
  }
}
