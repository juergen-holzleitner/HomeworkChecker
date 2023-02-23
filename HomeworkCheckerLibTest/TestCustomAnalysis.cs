using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System;
using System.Collections.Generic;
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
      fileEnumeratorMock.Setup(f => f.ReadFileContent("file1.java")).Returns("");
      fileEnumeratorMock.Setup(f => f.ReadFileContent("file2.java")).Returns("");

      var sut = new CustomAnalysisProcessor(new FilesystemService(fileEnumeratorMock.Object));

      var result = sut.Process(new List<string> { "file1.java", "file2.java" });

      result.Should().Be(new CustomAnalysisProcessor.CustomAnalysisResult(string.Empty));
    }

    [Theory]
    [InlineData("str.format(\"abcd\");", "file.java", 3, "file.java (3): use of format without %")]
    [InlineData("println(\"\")", "file.java", 1, "file.java (1): use of empty string")]
    [InlineData("printf", "file.java", 1, "file.java (1): use of printf instead of format")]
    [InlineData("\"\"", "file.java", 1, "file.java (1): use of empty string")]
    [InlineData("\" %n\"", "file.java", 1, "file.java (1): space before newline")]
    [InlineData("\\n", "file.java", 1, "file.java (1): use of \\n instead of %n")]
    [InlineData("format(\"%n\")", "file.java", 1, "file.java (1): format is used to print a newline")]
    [InlineData("\"X\"", "file.java", 1, "file.java (1): use of string for a single character")]
    [InlineData("x+= 1", "file.java", 1, "file.java (1): use of += instead of ++")]
    [InlineData("x -=1", "file.java", 1, "file.java (1): use of -= instead of --")]

    public void Can_detect_custom_errors(string codeLine, string fileName, int lineNumber, string expectedError)
    {
      var sb = new StringBuilder();

      CustomAnalysisProcessor.AnalyzeLine(fileName, lineNumber, codeLine, sb);

      sb.ToString().Should().Be(expectedError + Environment.NewLine);
    }
  }
}
