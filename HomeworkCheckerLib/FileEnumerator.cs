namespace HomeworkCheckerLib
{
  internal class FileEnumerator : DirectoryService.IFileEnumerator
  {
    public IEnumerable<string> GetFilesInFolderRecursivly(string folder, string extension)
    {
      return Directory.GetFiles(folder, extension, searchOption: SearchOption.AllDirectories);
    }
  }
}
