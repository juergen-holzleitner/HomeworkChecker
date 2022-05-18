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

    internal static string FromSubmissionAnalysis(HomeworkChecker.SubmissionAnalysis analysisResult, string baseFolder)
    {
      var sb = new StringBuilder();

      AppendFileNameIssues(sb, analysisResult.FileNameAnalysis);

      AppendDuplicateIssues(sb, analysisResult.Similarities.Duplicates, baseFolder);

      AppendJplagSimilarities(sb, analysisResult.Similarities.JplagSimilarities, analysisResult.Similarities.JplagMasterSimilarity, baseFolder);

      AppendOutputDifferences(sb, analysisResult.OutputDifference);

      AppendFileAnalysisProblems(sb, analysisResult.AnalysisResult);

      return sb.ToString();
    }

    internal static void AppendDuplicateIssues(StringBuilder sb, IEnumerable<DuplicateFileAnalyzer.Similarity> duplicates, string baseFolder)
    {
      if (!duplicates.Any())
        return;

      var d = duplicates.Select(d => $"{GenerateShortFileName(d.FilePath, baseFolder)} ({d.SimilarityMode})");
      AppendAnalysisIssue(sb, "duplicate problems", string.Join(Environment.NewLine, d));
    }
    
    private static string GenerateShortFileName(string javaFile, string baseFolder)
    {
      var outputName = javaFile[baseFolder.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      return outputName;
    }

    internal static void AppendJplagSimilarities(StringBuilder sb, IEnumerable<JplagProcessor.SubmissionSimilarity> jplagSimilarities, JplagProcessor.SubmissionSimilarity? masterSimilarity, string baseFolder)
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
          sb.AppendLine($"\t{pos}. {GenerateShortFileName(f.File, baseFolder)}: {f.Similarity}");
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
      AppendHtmlFromDiff(sb, fileNameAnalysis.FileNameDifference.Diffs);
      sb.Append($" ({fileNameAnalysis.Name} | {fileNameAnalysis.ExpectedName})");
      sb.AppendLine("</code></pre>");
      sb.AppendLine();
    }

    private static void AppendFileAnalysisProblems(StringBuilder sb, HomeworkChecker.FileAnalysisResult analysisResult)
    {
      AppendAnalysisIssue(sb, "compile problems", analysisResult.CompileIssues);
      AppendOutputProblems(sb, analysisResult.Outputs);
      AppendAnalysisIssue(sb, "Custom problems", analysisResult.CustomAnalysisIssues);
      AppendSpotBugsIssue(sb, analysisResult.SpotBugsIssues);
      AppendAnalysisIssue(sb, "PMD problems", analysisResult.PMDIssues);
      AppendCheckstyleIssue(sb, analysisResult.CheckstyleIssues);
    }

    internal static void AppendOutputProblems(StringBuilder sb, IEnumerable<HomeworkChecker.Output> outputs)
    {
      var outputsWithProblems = outputs.Where(o => o.ExitCode != 0);
      if (outputsWithProblems.Any())
      {
        sb.AppendLine("## output problems");
        sb.AppendLine();

        foreach (var output in outputsWithProblems)
        {
          sb.AppendLine($"generation for {output.Input.Filename} has <span style=\"color:Red;font-weight:bold\">ExitCode {output.ExitCode}</span>");
          sb.AppendLine();

          if (!string.IsNullOrEmpty(output.Input.FileContent))
          {
            var inputContent = new StringBuilder();
            inputContent.AppendLine("```");
            inputContent.AppendLine(output.Input.FileContent);
            inputContent.AppendLine("```");
            AppendExpandableBlock(sb, "input", inputContent.ToString());
          }

          var outputContent = new StringBuilder();
          outputContent.AppendLine("```");
          outputContent.AppendLine(output.OutputContent);
          outputContent.AppendLine("```");
          AppendExpandableBlock(sb, "output", outputContent.ToString());
        }
      }
    }

    internal static void AppendAnalysisIssue(StringBuilder sb, string header, string issueText)
    {
      if (string.IsNullOrEmpty(issueText))
        return;

      var trimmedIssueText = issueText.Trim('\r', '\n');

      sb.AppendLine("## " + header);
      sb.AppendLine();
      sb.Append("<pre><code>");
      sb.Append(trimmedIssueText);
      sb.AppendLine("</code></pre>");
      sb.AppendLine();
    }

    private static void AppendHtmlFromDiff(StringBuilder sb, List<DiffMatchPatch.Diff> diffs)
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

    internal static void AppendOutputDifferences(StringBuilder sb, OutputDifferencesAnalyzer.OutputDifferenceAnalysis outputDifference)
    {
      sb.AppendLine("## output analysis");
      sb.AppendLine();

      foreach (var diff in outputDifference.Differences)
      {
        sb.AppendLine("<details>");
        var headLine = diff.DifferenceType == OutputDifferencesAnalyzer.DifferenceType.Equal ? $"output for {diff.SubmissionOutput.Input.Filename} is good" : $"output for {diff.SubmissionOutput.Input.Filename} differs";
        if (diff.DifferenceType == OutputDifferencesAnalyzer.DifferenceType.Equal)
          headLine = $"<span style=\"color:DarkGreen;\">{headLine}</span>";
        else if (diff.DifferenceType == OutputDifferencesAnalyzer.DifferenceType.WhitespaceOnly)
          headLine = $"<span style=\"color:Yellow;font-weight:bold;\">{headLine} (whitespace only)</span>";
        else
          headLine = $"<span style=\"color:Red;font-weight:bold;\">{headLine}</span>";
        sb.AppendLine($"  <summary>{headLine}</summary>");
        sb.AppendLine();

        if (diff.SubmissionOutput.HasTimedOut)
        {
          sb.AppendLine("**execution timed out**");
          sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(diff.SubmissionOutput.Input.FileContent))
        {
          var input = new StringBuilder();
          input.AppendLine("```");
          input.AppendLine(diff.SubmissionOutput.Input.FileContent);
          input.AppendLine("```");
          AppendExpandableBlock(sb, "input", input.ToString());
        }

        if (diff.DifferenceType == OutputDifferencesAnalyzer.DifferenceType.Equal)
        {
          var output = new StringBuilder();
          output.AppendLine("```");
          output.AppendLine(diff.SubmissionOutput.OutputContent);
          output.AppendLine("```");
          AppendExpandableBlock(sb, "output", output.ToString());
        }
        else
        {
          var output = new StringBuilder();
          output.AppendLine("expected");
          output.AppendLine("```");
          output.AppendLine(diff.MasterOutput.OutputContent);
          output.AppendLine("```");
          output.AppendLine("actual");
          output.AppendLine("```");
          output.AppendLine(diff.SubmissionOutput.OutputContent);
          output.AppendLine("```");
          AppendExpandableBlock(sb, "output", output.ToString());

          sb.Append("<pre><code>");
          AppendHtmlFromDiff(sb, diff.Difference.Diffs);
          sb.AppendLine("</code></pre>");
        }

        sb.AppendLine("</details>");
        sb.AppendLine();
      }

    }

    internal static void AppendExpandableBlock(StringBuilder sb, string header, string content)
    {
      sb.AppendLine("<details>");
      sb.AppendLine($"  <summary>Click to expand {header}</summary>");
      sb.AppendLine();
      sb.AppendLine(content);
      sb.AppendLine("</details>");
      sb.AppendLine();
    }

    internal static void AppendCheckstyleIssue(StringBuilder sb, string checkstyleResult)
    {
      var result = new StringBuilder();
      using (var reader = new StringReader(checkstyleResult))
      {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
          bool isImportant = true;
          if (line.Contains("[WhitespaceAround]"))
            isImportant = false;
          if (line.Contains("[ParenPad]"))
            isImportant = false;
          if (line.Contains("[WhitespaceAfter]"))
            isImportant = false;
          if (line.Contains("[MethodParamPad]"))
            isImportant = false;
          if (line.Contains("[NoWhitespaceBefore]"))
            isImportant = false;

          if (isImportant)
            result.Append(@"<span style=""color:Yellow;font-weight:bold;"">");
          result.Append(line);
          if (isImportant)
            result.Append(@"</span>");
          result.AppendLine();
        }
      }

      AppendAnalysisIssue(sb, "checkstyle problems", result.ToString());
    }

    internal static void AppendSpotBugsIssue(StringBuilder sb, string spotBugsResult)
    {
      var result = new StringBuilder();
      result.Append(@"<span style=""color:Yellow;font-weight:bold;"">");
      result.Append(spotBugsResult);
      result.Append("</span>");
      AppendAnalysisIssue(sb, "SpotBugs problems", result.ToString());
    }
  }
}
