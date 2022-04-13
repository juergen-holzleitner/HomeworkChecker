using System.Linq;

namespace HomeworkCheckerLib
{
  internal class DirectoryService
  {
    internal interface IFileEnumerator
    {
      IEnumerable<string> GetFilesInFolderRecursivly(string folder, string extension);
    }

    IFileEnumerator fileEnumerator;

    public DirectoryService(IFileEnumerator fileEnumerator)
    {
      this.fileEnumerator = fileEnumerator;
    }

    internal IEnumerable<string> GetAllHomeWorkFolders(string folder)
    {
      var files = fileEnumerator.GetFilesInFolderRecursivly(folder, "*.java");
      return files.Select(f => Path.GetDirectoryName(f)!);
    }
  }
}
