using System.Diagnostics;
using System.Text.RegularExpressions;

namespace HomeworkCheckerLib
{
  public class JplagProcessor
  {
    private readonly IAppExecuter appExecuter;
    private readonly FilesystemService.IFileEnumerator filesystemService;

    public record JplagResult(IEnumerable<JplagSimilarity> Similarities);

    internal JplagProcessor(IAppExecuter appExecuter, FilesystemService.IFileEnumerator filesystemService)
    {
      this.appExecuter = appExecuter;
      this.filesystemService = filesystemService;
    }

    internal JplagResult Process(string masterFolder, string homeworkFolder)
    {
      const string jplagResultFolder = "TEMP-jplag-result";

      var currentFolder = appExecuter.GetCurrentFolder();
      var workingFolder = Path.Combine(currentFolder, "jplag");
      var result = appExecuter.Execute("java", workingFolder, $"-jar jplag-4.0.0-SNAPSHOT-jar-with-dependencies.jar -m 100 -t 4 -r \"{jplagResultFolder}\" \"{masterFolder}\" \"{homeworkFolder}\"");

      Trace.Assert(result.ExitCode == 0, $"jplag is not expected to return {result.ExitCode}");

      filesystemService.RemoveFolderIfExists(Path.Combine(workingFolder, jplagResultFolder));

      var similarities = GetSimilarities(result.Output);

      return new(similarities);
    }

    public record JplagSimilarity(string FileA, string FileB, double Similarity);

    internal static IEnumerable<JplagSimilarity> GetSimilarities(string jplagOutput)
    {
      var regex = new Regex(@"Comparing ""(?<fileA>.+)"" - ""(?<fileB>.+)"": (?<result>.+)");
      var matches = regex.Matches(jplagOutput);
      foreach (Match match in matches)
      {
        var fileA = match.Groups["fileA"].Value;
        var fileB = match.Groups["fileB"].Value;
        var result = match.Groups["result"].Value;
        var similarity = double.Parse(result);
        yield return new JplagSimilarity(fileA, fileB, similarity);
      }
    }

    internal static int GetExpectedNumberOfSimilarities(int numberOfFiles)
    {
      return numberOfFiles * (numberOfFiles - 1) / 2;
    }

    public record SubmissionSimilarity(string File, double Similarity);

    internal static IEnumerable<SubmissionSimilarity> GetSubmissionSimilarities(string submissionFile, IEnumerable<JplagSimilarity> similarities)
    {
      var resultA = from s in similarities where s.FileA == submissionFile select new SubmissionSimilarity(s.FileB, s.Similarity);
      var resultB = from s in similarities where s.FileB == submissionFile select new SubmissionSimilarity(s.FileA, s.Similarity);

      return Enumerable.Union(resultA, resultB);
    }
  }
}
