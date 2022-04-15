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
      var files = GetAllJavaFiles(folder);
      return files.Select(f => Path.GetDirectoryName(f)!).Distinct();
    }

    internal IEnumerable<string> GetAllJavaFiles(string folder)
    {
      return fileEnumerator.GetFilesInFolderRecursivly(folder, "*.java");
    }
  }
}
