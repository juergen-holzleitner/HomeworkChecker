namespace HomeworkCheckerLib
{
  public class TextDiffGenerator
  {
    public record Difference(List<DiffMatchPatch.Diff> Diffs);

    internal static Difference GenerateDiff(string oldText, string newText)
    {
      if (oldText == newText)
        return new(new());

      var dmp = new DiffMatchPatch.diff_match_patch();
      var diffs = dmp.diff_main(oldText, newText, false);
      dmp.diff_cleanupSemantic(diffs);

      return new Difference(diffs);
    }

    internal static string GetStringWithoutWhiteSpaces(string fileContent)
    {
      return string.Concat(fileContent.Where(c => !char.IsWhiteSpace(c)));
    }
  }
}
