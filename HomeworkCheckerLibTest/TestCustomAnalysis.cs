using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System;
using System.Text;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestCustomAnalysis
  {
    [Fact]
    public void Can_process_CustomAnalysis()
    {
      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(f => f.ReadFileContent("file.java")).Returns("");

      var sut = new CustomAnalysisProcessor(new FilesystemService(fileEnumeratorMock.Object));

      var result = sut.Process("file.java");

      result.Should().Be(new CustomAnalysisProcessor.CustomAnalysisResult(string.Empty));
    }

    [Theory]
    [InlineData("str.format(\"abcd\");", 3, "3: use of format without %")]
    [InlineData("println(\"\")", 1, "1: use of empty string")]
    [InlineData("printf", 1, "1: use of printf instead of format")]
    [InlineData("\"\"", 1, "1: use of empty string")]
    [InlineData("\" %n\"", 1, "1: space before newline")]
    [InlineData("\\n", 1, "1: use of \\n instead of %n")]
    [InlineData("format(\"%n\")", 1, "1: format is used to print a newline")]
    [InlineData("\"X\"", 1, "1: use of string for a single character")]
    [InlineData("x+= 1", 1, "1: use of += instead of ++")]
    [InlineData("x -=1", 1, "1: use of -= instead of --")]

    public void Can_detect_custom_errors(string codeLine, int lineNumber, string expectedError)
    {
      var sb = new StringBuilder();

      CustomAnalysisProcessor.AnalyzeLine(lineNumber, codeLine, sb);

      sb.ToString().Should().Be(expectedError + Environment.NewLine);
    }
  }
}
