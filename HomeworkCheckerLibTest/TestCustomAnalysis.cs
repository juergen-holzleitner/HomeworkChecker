using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
  }
}
