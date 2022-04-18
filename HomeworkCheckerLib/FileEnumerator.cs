namespace HomeworkCheckerLib
{
  internal class FileEnumerator : FilesystemService.IFileEnumerator
  {
    public IEnumerable<string> GetFilesInFolderRecursivly(string folder, string extension)
    {
      return Directory.GetFiles(folder, extension, searchOption: SearchOption.AllDirectories);
    }

    public string ReadFileContent(string filePath)
    {
      return File.ReadAllText(filePath);
    }

    public void RemoveFolderIfExists(string folder)
    {
      if (Directory.Exists(folder))
        Directory.Delete(folder, true);
    }

    public void RemoveFileIfExists(string filePath)
    {
      if (File.Exists(filePath))
        File.Delete(filePath);
    }

    public void AppendAllText(string fileName, string markdown)
    {
      File.AppendAllText(fileName, markdown);
    }
  }
}
