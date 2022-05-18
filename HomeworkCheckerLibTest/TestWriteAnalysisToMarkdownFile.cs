using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestWriteAnalysisToMarkdownFile
  {
    [Fact]
    public void Can_write_analysis_to_markdown()
    {
      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      var analysisResult = new HomeworkChecker.FileAnalysisResult(@"outputFolder\someFile.java", "<<<compile>>>", new List<HomeworkChecker.Output> { new(new("inputfile", "<<<input>>>"), 1, "<<<output>>>", false) }, "<<<checkstyle>>>", "<<<PMD>>>", "<<<SpotBugs>>>", "<<<custom>>>");
      var sut = new HomeworkChecker(fileEnumeratorMock.Object, Mock.Of<IAppExecuter>(), Mock.Of<IRuntimeOutput>());

      sut.WriteAnalysisToMarkdownFile(analysisResult);

      System.Linq.Expressions.Expression<Func<string, bool>> verifyAllPartsIncluded = s =>
        s.Contains("<<<compile>>>")
        && s.Contains("<<<custom>>>")
        && s.Contains("<<<SpotBugs>>>")
        && s.Contains("<<<PMD>>>")
        && s.Contains("<<<checkstyle>>>")
        ;
      fileEnumeratorMock.Verify(f => f.AppendAllText(@"outputFolder\NOTES.md", It.Is(verifyAllPartsIncluded)));
    }

    [Fact]
    public void Empty_output_is_not_appended()
    {
      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      var sut = new FilesystemService(fileEnumeratorMock.Object);

      sut.AppendMarkdown("someFile.java", string.Empty);

      fileEnumeratorMock.Verify(f => f.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Can_generate_markdown_for_issue()
    {
      var sb = new StringBuilder();

      MarkdownGenerator.AppendAnalysisIssue(sb, "compile problems", "\r\ncompile error\n");

      sb.ToString().Should().Be(
@"## compile problems

<pre><code>compile error</code></pre>

");
    }

    [Fact]
    public void Can_write_submission_analysis_to_markdown()
    {
      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      var analysisResult = new HomeworkChecker.FileAnalysisResult(@"outputFolder\someFile.java", "<<<compile>>>", new List<HomeworkChecker.Output> { new(new("inputfile", "<<<input>>>"), 0, "<<<output>>>", false) }, "<<<checkstyle>>>", "<<<PMD>>>", "<<<SpotBugs>>>", "<<<custom>>>");
      var duplicates = new List<DuplicateFileAnalyzer.Similarity> { new("outputFolder\\file", DuplicateFileAnalyzer.SimilarityMode.WhitespaceDifferences) };
      var jplagSimilarities = new List<JplagProcessor.SubmissionSimilarity>();
      var similarities = new HomeworkChecker.SimilarityAnalysis(duplicates, jplagSimilarities, new("Master.java", 99));
      var fileNameDiffs = new List<DiffMatchPatch.Diff>() { new(DiffMatchPatch.Operation.INSERT, "xxx"), new(DiffMatchPatch.Operation.DELETE, "java") };
      var filenameDifferences = new TextDiffGenerator.Difference(fileNameDiffs);
      var fileNameAnalysis = new HomeworkChecker.FileNameAnalysis("java", "xxx", new(fileNameDiffs));
      var input = new HomeworkChecker.Input("inputFile.txt", "input content");
      HomeworkChecker.Output masterOutput = new(input, 0, "outputcontent", false);
      HomeworkChecker.Output submissionOutput = new(input, 0, "output content", false);
      var ouputDiffs = fileNameDiffs;
      var outputDifferences = new List<OutputDifferencesAnalyzer.OutputDifference> { new OutputDifferencesAnalyzer.OutputDifference(masterOutput, submissionOutput, OutputDifferencesAnalyzer.DifferenceType.Different, new TextDiffGenerator.Difference(ouputDiffs)) };
      var outputDifference = new OutputDifferencesAnalyzer.OutputDifferenceAnalysis(outputDifferences);
      var submissionAnalysis = new HomeworkChecker.SubmissionAnalysis(similarities, fileNameAnalysis, outputDifference, analysisResult);
      var homeworkResult = new HomeworkChecker.HomeworkResult(analysisResult, "outputFolder", new List<HomeworkChecker.SubmissionAnalysis> { submissionAnalysis });
      var sut = new HomeworkChecker(fileEnumeratorMock.Object, Mock.Of<IAppExecuter>(), Mock.Of<IRuntimeOutput>());

      sut.WriteAnalysisToMarkdownFile(homeworkResult);

      System.Linq.Expressions.Expression<Func<string, bool>> verifyAllPartsIncluded = s =>
        s.Contains("<<<compile>>>")
        && s.Contains("filename problems")
        && s.Contains("duplicate problems")
        && s.Contains("output analysis")
        && s.Contains("inputFile.txt")
        && s.Contains("input content")
        && s.Contains("outputcontent")
        && s.Contains("output content")
        && s.Contains("Jplag similarities")
        && s.Contains("<<<custom>>>")
        && s.Contains("<<<SpotBugs>>>")
        && s.Contains("<<<PMD>>>")
        && s.Contains("<<<checkstyle>>>")
        ;
      fileEnumeratorMock.Verify(f => f.AppendAllText(@"outputFolder\NOTES.md", It.Is(verifyAllPartsIncluded)));
    }

    [Fact]
    public void Can_generate_filename_issues()
    {
      var sb = new StringBuilder();
      var fileNameDiffs = new List<DiffMatchPatch.Diff>()
      {
        new(DiffMatchPatch.Operation.DELETE, "f"),
        new(DiffMatchPatch.Operation.INSERT, "F"),
        new(DiffMatchPatch.Operation.EQUAL, "ile.java")
      };

      var fileNameAnalysis = new HomeworkChecker.FileNameAnalysis("file.java", "File.java", new(fileNameDiffs));

      MarkdownGenerator.AppendFileNameIssues(sb, fileNameAnalysis);

      sb.ToString().Should().Be(
@"## filename problems

<pre><code><del style=""color:Red;font-weight:bold;"">f</del><ins style=""color:DarkGreen;font-weight:bold;"">F</ins><span>ile.java</span> (file.java | File.java)</code></pre>

");
    }

    [Fact]
    public void Can_generate_duplicate_issues()
    {
      var duplicates = new List<DuplicateFileAnalyzer.Similarity>
      {
        new DuplicateFileAnalyzer.Similarity(@"homeworkFolder\filePath.java", DuplicateFileAnalyzer.SimilarityMode.ExactCopy),
        new DuplicateFileAnalyzer.Similarity(@"homeworkFodler\filePath2.java", DuplicateFileAnalyzer.SimilarityMode.WhitespaceDifferences),
      };
      var sb = new StringBuilder();

      MarkdownGenerator.AppendDuplicateIssues(sb, duplicates, "homeworkFolder");

      sb.ToString().Should().Be(@"## duplicate problems

<pre><code>filePath.java (ExactCopy)
filePath2.java (WhitespaceDifferences)</code></pre>

");
    }

    [Fact]
    public void Can_generate_Jplag_similarities()
    {
      var jplagSimilarities = new List<JplagProcessor.SubmissionSimilarity>()
      {
        new JplagProcessor.SubmissionSimilarity("homeworkFolder\\fileA.java", 99),
        new JplagProcessor.SubmissionSimilarity("homeworkFolder\\fileB.java", 100),
        new JplagProcessor.SubmissionSimilarity("homeworkFolder\\fileC.java", 50),
      };
      var masterSimilarity = new JplagProcessor.SubmissionSimilarity("Master.java", 53.0);
      var sb = new StringBuilder();

      MarkdownGenerator.AppendJplagSimilarities(sb, jplagSimilarities, masterSimilarity, "homeworkFolder");

      sb.ToString().Should().Be(@"## Jplag similarities

* similarity with master: **53**

* similar files
	1. fileB.java: 100
	2. fileA.java: 99
	3. fileC.java: 50

");
    }

    [Fact]
    public void Can_write_output_differences()
    {
      var input = new HomeworkChecker.Input("inputFile.txt", "input content");
      HomeworkChecker.Output masterOutput = new(input, 0, "ouputcontent", false);
      HomeworkChecker.Output submissionOutput = new(input, 0, "ouput content", false);
      var ouputDiffs = new List<DiffMatchPatch.Diff>()
      {
        new(DiffMatchPatch.Operation.DELETE, "C"),
        new(DiffMatchPatch.Operation.INSERT, "c"),
        new(DiffMatchPatch.Operation.EQUAL, "ode")
      };
      var outputDifferences = new List<OutputDifferencesAnalyzer.OutputDifference> { new OutputDifferencesAnalyzer.OutputDifference(masterOutput, submissionOutput, OutputDifferencesAnalyzer.DifferenceType.Different, new TextDiffGenerator.Difference(ouputDiffs)) };
      var outputDifference = new OutputDifferencesAnalyzer.OutputDifferenceAnalysis(outputDifferences);

      var sb = new StringBuilder();

      MarkdownGenerator.AppendOutputDifferences(sb, outputDifference);

      sb.ToString().Should().Be(@"## output analysis

<details>
  <summary><span style=""color:Red;font-weight:bold;"">output for inputFile.txt differs</span></summary>

<details>
  <summary>Click to expand input</summary>

```
input content
```

</details>

<details>
  <summary>Click to expand output</summary>

expected
```
ouputcontent
```
actual
```
ouput content
```

</details>

<pre><code><del style=""color:Red;font-weight:bold;"">C</del><ins style=""color:DarkGreen;font-weight:bold;"">c</ins><span>ode</span></code></pre>
</details>

");
    }

        [Fact]
    public void Can_write_output_difference_with_whitespace_only()
    {
      var input = new HomeworkChecker.Input("inputFile.txt", "input content");
      HomeworkChecker.Output masterOutput = new(input, 0, "ouputcontent", false);
      HomeworkChecker.Output submissionOutput = new(input, 0, "ouput content", false);
      var ouputDiffs = new List<DiffMatchPatch.Diff>()
      {
        new(DiffMatchPatch.Operation.DELETE, " "),
      };
      var outputDifferences = new List<OutputDifferencesAnalyzer.OutputDifference> { new OutputDifferencesAnalyzer.OutputDifference(masterOutput, submissionOutput, OutputDifferencesAnalyzer.DifferenceType.WhitespaceOnly, new TextDiffGenerator.Difference(ouputDiffs)) };
      var outputDifference = new OutputDifferencesAnalyzer.OutputDifferenceAnalysis(outputDifferences);

      var sb = new StringBuilder();

      MarkdownGenerator.AppendOutputDifferences(sb, outputDifference);

      sb.ToString().Should().Be(@"## output analysis

<details>
  <summary><span style=""color:Yellow;font-weight:bold;"">output for inputFile.txt differs (whitespace only)</span></summary>

<details>
  <summary>Click to expand input</summary>

```
input content
```

</details>

<details>
  <summary>Click to expand output</summary>

expected
```
ouputcontent
```
actual
```
ouput content
```

</details>

<pre><code><del style=""color:Red;font-weight:bold;""> </del></code></pre>
</details>

");
    }

    [Fact]
    public void Can_generate_expandable_block()
    {
      var sb = new StringBuilder();

      MarkdownGenerator.AppendExpandableBlock(sb, "header", "content");

      sb.ToString().Should().Be(@"<details>
  <summary>Click to expand header</summary>

content
</details>

");
    }

    [Fact]
    public void Can_generate_output_problems()
    {
      var input = new HomeworkChecker.Input("inputFile.txt", "input content");
      HomeworkChecker.Output output = new(input, -1, "ouputcontent", false);

      var sb = new StringBuilder();
      MarkdownGenerator.AppendOutputProblems(sb, new List<HomeworkChecker.Output> { output });

      sb.ToString().Should().Be(@"## output problems

generation for inputFile.txt has <span style=""color:Red;font-weight:bold"">ExitCode -1</span>

<details>
  <summary>Click to expand input</summary>

```
input content
```

</details>

<details>
  <summary>Click to expand output</summary>

```
ouputcontent
```

</details>

");
    }

    [Fact]
    public void Can_generate_colored_checkstyle_output()
    {
      var checkstyleResult = @"[WARN] blabla [Important]
[WARN] blabla whitespace [WhitespaceAround]
[WARN] blabla whitespace [MethodParamPad]
[WARN] blabla whitespace [NoWhitespaceBefore]
[WARN] blabla whitespace [WhitespaceAfter]";
      var sb = new StringBuilder();

      MarkdownGenerator.AppendCheckstyleIssue(sb, checkstyleResult);

      sb.ToString().Should().Be(@"## checkstyle problems

<pre><code><span style=""color:Yellow;font-weight:bold;"">[WARN] blabla [Important]</span>
[WARN] blabla whitespace [WhitespaceAround]
[WARN] blabla whitespace [MethodParamPad]
[WARN] blabla whitespace [NoWhitespaceBefore]
[WARN] blabla whitespace [WhitespaceAfter]</code></pre>

");

    }

    [Fact]
    public void Can_generate_colored_SpotBugs_output()
    {
      var spotBugsResult = @"blabla
blablabla";
      var sb = new StringBuilder();

      MarkdownGenerator.AppendSpotBugsIssue(sb, spotBugsResult);

      sb.ToString().Should().Be(@"## SpotBugs problems

<pre><code><span style=""color:Yellow;font-weight:bold;"">blabla
blablabla</span></code></pre>

");

    }

    [Fact]
    public void Can_generate_colored_PMD_output()
    {
      var pmdResult = @"UselessParentheses:	Useless parentheses.
ForLoopCanBeForeach:	This for loop can be replaced by a foreach loop
OnlyOneReturn:	A method should have only one exit point, and that should be the last statement in the method
LocalVariableCouldBeFinal:	Local variable";
      var sb = new StringBuilder();

      MarkdownGenerator.AppendPMDIssue(sb, pmdResult);

      sb.ToString().Should().Be(@"## PMD problems

<pre><code><span style=""color:Yellow;font-weight:bold;"">UselessParentheses:	Useless parentheses.</span>
ForLoopCanBeForeach:	This for loop can be replaced by a foreach loop
OnlyOneReturn:	A method should have only one exit point, and that should be the last statement in the method
LocalVariableCouldBeFinal:	Local variable</code></pre>

");

    }

    [Fact]
    public void Can_generate_colored_CustomAnalysis_output()
    {
      var customAnalysisResult = @"blabla
blablabla";
      var sb = new StringBuilder();

      MarkdownGenerator.AppendCustomAnalyssisIssue(sb, customAnalysisResult);

      sb.ToString().Should().Be(@"## Custom problems

<pre><code><span style=""color:Yellow;font-weight:bold;"">blabla
blablabla</span></code></pre>

");

    }

  }
}
