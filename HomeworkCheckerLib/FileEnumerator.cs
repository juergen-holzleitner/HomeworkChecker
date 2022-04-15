namespace HomeworkCheckerLib
{
  internal class FileEnumerator : FilesystemService.IFileEnumerator
  {
    public IEnumerable<string> GetFilesInFolderRecursivly(string folder, string extension)
    {
      return Directory.GetFiles(folder, extension, searchOption: SearchOption.AllDirectories);
    }
  }
}
