using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeworkCheckerLib
{
  internal class TextDiffGenerator
  {
    public record Difference(List<DiffMatchPatch.Diff> Diffs);

    internal static Difference GenerateDiff(string oldText, string newText)
    {
      var dmp = new DiffMatchPatch.diff_match_patch();
      var diffs = dmp.diff_main(oldText, newText, false);
      dmp.diff_cleanupSemantic(diffs);

      return new Difference(diffs);
    }
  }
}
