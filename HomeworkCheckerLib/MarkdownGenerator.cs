using System.Text;

namespace HomeworkCheckerLib
{
  internal class MarkdownGenerator
  {
    internal static string FromFileAnalysis(HomeworkChecker.FileAnalysisResult analysisResult)
    {
      var sb = new StringBuilder();

      AppendFileAnalysisProblems(sb, analysisResult);

      return sb.ToString();
    }

    internal static string FromSubmissionAnalysis(HomeworkChecker.SubmissionAnalysis analysisResult)
    {
      var sb = new StringBuilder();

      AppendFileNameIssues(sb, analysisResult.FileNameAnalysis);

      AppendDuplicateIssues(sb, analysisResult.Similarities.Duplicates);

      AppendJplagSimilarities(sb, analysisResult.Similarities.JplagSimilarities, analysisResult.Similarities.JplagMasterSimilarity);

      AppendFileAnalysisProblems(sb, analysisResult.AnalysisResult);

      return sb.ToString();
    }

    internal static void AppendDuplicateIssues(StringBuilder sb, IEnumerable<DuplicateFileAnalyzer.Similarity> duplicates)
    {
      if (!duplicates.Any())
        return;

      var d = duplicates.Select(d => $"{d.FilePath} ({d.SimilarityMode})");
      AppendAnalysisIssue(sb, "duplicate problems", string.Join(Environment.NewLine, d));
    }
    internal static void AppendJplagSimilarities(StringBuilder sb, IEnumerable<JplagProcessor.SubmissionSimilarity> jplagSimilarities, JplagProcessor.SubmissionSimilarity? masterSimilarity)
    {
      if (!jplagSimilarities.Any() && masterSimilarity is null)
        return;

      sb.AppendLine("## Jplag similarities");
      sb.AppendLine();

      if (masterSimilarity is not null)
      {
        sb.AppendLine($"* similarity with master: **{masterSimilarity.Similarity}**");
        sb.AppendLine();
      }

      if (jplagSimilarities.Any())
      {
        var mostSimilar = (from j in jplagSimilarities orderby j.Similarity descending select j).TakeWhile(j => j.Similarity >= 90);
        if (mostSimilar.Count() < 3)
          mostSimilar = jplagSimilarities.OrderByDescending(j => j.Similarity).Take(3);

        int pos = 1;
        sb.AppendLine("* similar files");
        foreach (var f in mostSimilar)
        {
          sb.AppendLine($"\t{pos}. {f.File}: {f.Similarity}");
          ++pos;
        }
        sb.AppendLine();
      }
    }

    internal static void AppendFileNameIssues(StringBuilder sb, HomeworkChecker.FileNameAnalysis fileNameAnalysis)
    {
      if (!fileNameAnalysis.FileNameDifference.Diffs.Any())
        return;

      sb.AppendLine("## filename problems");
      sb.AppendLine();

      sb.Append("<pre><code>");
      AppendHtmlFromDiff(fileNameAnalysis.FileNameDifference.Diffs, sb);
      sb.Append($" ({fileNameAnalysis.Name} | {fileNameAnalysis.ExpectedName})");
      sb.AppendLine("</code></pre>");
      sb.AppendLine();
    }

    private static void AppendFileAnalysisProblems(StringBuilder sb, HomeworkChecker.FileAnalysisResult analysisResult)
    {
      AppendAnalysisIssue(sb, "compile problems", analysisResult.CompileIssues);
      AppendAnalysisIssue(sb, "Custom problems", analysisResult.CustomAnalysisIssues);
      AppendAnalysisIssue(sb, "SpotBugs problems", analysisResult.SpotBugsIssues);
      AppendAnalysisIssue(sb, "PMD problems", analysisResult.PMDIssues);
      AppendAnalysisIssue(sb, "checkstyle problems", analysisResult.CheckstyleIssues);
    }

    internal static void AppendAnalysisIssue(StringBuilder sb, string header, string issueText)
    {
      if (string.IsNullOrEmpty(issueText))
        return;

      var timmedIssueText = issueText.Trim('\r', '\n');

      sb.AppendLine("## " + header);
      sb.AppendLine();
      sb.AppendLine("```");
      sb.AppendLine(timmedIssueText);
      sb.AppendLine("```");
      sb.AppendLine();
    }

    private static void AppendHtmlFromDiff(List<DiffMatchPatch.Diff> diffs, StringBuilder sb)
    {
      // based on diff_prettyHtml(List<Diff> diffs)
      foreach (var aDiff in diffs)
      {
        string text = aDiff.text;
        switch (aDiff.operation)
        {
          case DiffMatchPatch.Operation.INSERT:
            sb.Append("<ins style=\"color:DarkGreen;font-weight:bold;\">").Append(text.Replace(Environment.NewLine, " " + Environment.NewLine))
                .Append("</ins>");
            break;
          case DiffMatchPatch.Operation.DELETE:
            sb.Append("<del style=\"color:Red;font-weight:bold;\">").Append(text.Replace(Environment.NewLine, " " + Environment.NewLine))
                .Append("</del>");
            break;
          case DiffMatchPatch.Operation.EQUAL:
            sb.Append("<span>").Append(text).Append("</span>");
            break;
        }
      }
    }

  }
}
