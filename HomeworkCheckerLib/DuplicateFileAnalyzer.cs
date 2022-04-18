namespace HomeworkCheckerLib
{
  public class DuplicateFileAnalyzer
  {
    private readonly FilesystemService filesystemService;

    internal DuplicateFileAnalyzer(FilesystemService filesystemService)
    {
      this.filesystemService = filesystemService;
    }

    public enum SimilarityMode { ExactCopy, WhitespaceDifferences };
    public record Similarity(string FilePath, SimilarityMode SimilarityMode);

    internal IEnumerable<Similarity> ProcessAnalysis(string file, IEnumerable<string> filesToAnalyze)
    {
      var fileContent = filesystemService.ReadFileContent(file);
      var fileContentWithoutWhiteSpaces = GetFileContentWithoutWhiteSpaces(fileContent);

      foreach (var f in filesToAnalyze.Where(f => f != file))
      {
        var otherContent = filesystemService.ReadFileContent(f);
        if (otherContent == fileContent)
          yield return new(f, SimilarityMode.ExactCopy);
        else if (GetFileContentWithoutWhiteSpaces(otherContent) == fileContentWithoutWhiteSpaces)
          yield return new(f, SimilarityMode.WhitespaceDifferences);
      }
    }

    private static string GetFileContentWithoutWhiteSpaces(string fileContent)
    {
      return string.Concat(fileContent.Where(c => !char.IsWhiteSpace(c)));
    }
  }
}
