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
      var fileContentWithoutWhiteSpaces = TextDiffGenerator.GetStringWithoutWhiteSpaces(fileContent);

      foreach (var f in filesToAnalyze.Where(f => f != file))
      {
        var otherContent = filesystemService.ReadFileContent(f);
        if (otherContent == fileContent)
          yield return new(f, SimilarityMode.ExactCopy);
        else if (TextDiffGenerator.GetStringWithoutWhiteSpaces(otherContent) == fileContentWithoutWhiteSpaces)
          yield return new(f, SimilarityMode.WhitespaceDifferences);
      }
    }

  }
}
