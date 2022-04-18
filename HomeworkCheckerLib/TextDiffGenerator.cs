namespace HomeworkCheckerLib
{
  public class TextDiffGenerator
  {
    public record Difference(List<DiffMatchPatch.Diff> Diffs);

    internal static Difference GenerateDiff(string oldText, string newText)
    {
      if (oldText == newText)
        return new(new());

      var dmp = new DiffMatchPatch.Diff_match_patch();
      var diffs = dmp.Diff_main(oldText, newText, false);
      dmp.Diff_cleanupSemantic(diffs);

      return new Difference(diffs);
    }
  }
}
