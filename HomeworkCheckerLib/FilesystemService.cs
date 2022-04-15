namespace HomeworkCheckerLib
{
  internal class FilesystemService
  {
    internal interface IFileEnumerator
    {
      IEnumerable<string> GetFilesInFolderRecursivly(string folder, string extension);
      string ReadFileContent(string filePath);
    }

    readonly IFileEnumerator fileEnumerator;

    public FilesystemService(IFileEnumerator fileEnumerator)
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

    internal string ReadFileContent(string filePath)
    {
      return fileEnumerator.ReadFileContent(filePath);
    }

    internal IEnumerable<string> GetAllInputFiles(string folder)
    {
      return fileEnumerator.GetFilesInFolderRecursivly(folder, "*.txt");
    }
  }
}
