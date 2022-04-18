using System.Text;

namespace HomeworkCheckerLib
{
  internal class MarkdownGenerator
  {
    internal static string FromFileAnalysis(HomeworkChecker.FileAnalysisResult analysisResult)
    {
      var sb = new StringBuilder();

      AppendAnalysisIssue(sb, "compile problems", analysisResult.CompileIssues);
      AppendAnalysisIssue(sb, "Custom problems", analysisResult.CustomAnalysisIssues);
      AppendAnalysisIssue(sb, "SpotBugs problems", analysisResult.SpotBugsIssues);
      AppendAnalysisIssue(sb, "PMD problems", analysisResult.PMDIssues);
      AppendAnalysisIssue(sb, "checkstyle problems", analysisResult.CheckstyleIssues);

      return sb.ToString();
    }

    internal static void AppendAnalysisIssue(StringBuilder sb, string header, string issueText)
    {
      if (string.IsNullOrEmpty(issueText))
        return;

      sb.AppendLine("## " + header);
      sb.AppendLine();
      sb.AppendLine("```");
      sb.AppendLine(issueText);
      sb.AppendLine("```");
      sb.AppendLine();
    }
  }
}
