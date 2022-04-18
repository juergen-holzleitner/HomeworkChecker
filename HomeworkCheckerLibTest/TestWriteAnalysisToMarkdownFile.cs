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
      var analysisResult = new HomeworkChecker.FileAnalysisResult(@"outputFolder\someFile.java", "<<<compile>>>", new List<HomeworkChecker.Output> { new(new("inputfile", "<<<input>>>"), "<<<output>>>", false) }, "<<<checkstyle>>>", "<<<PMD>>>", "<<<SpotBugs>>>", "<<<custom>>>");
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

```
compile error
```

");
    }

  }
}
