using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestDuplicateFileAnalyzer
  {
    [Fact]
    public void Can_call_DuplicateFileAnalyzer()
    {
      var filesToAnalyze = new List<string> { "fileA.java", "fileB.java" };
      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(f => f.ReadFileContent(It.IsAny<string>())).Returns("FileContent");

      var sut = new DuplicateFileAnalyzer(new FilesystemService(fileEnumeratorMock.Object));

      var duplicateAnalysis = sut.ProcessAnalysis("fileA.java", filesToAnalyze);

      duplicateAnalysis.Should().Equal(new List<DuplicateFileAnalyzer.Similarity> { new("fileB.java", DuplicateFileAnalyzer.SimilarityMode.ExactCopy) });
    }

    [Fact]
    public void Can_get_whitespace_only_differences()
    {
      var filesToAnalyze = new List<string> { "fileA.java", "fileB.java" };
      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(f => f.ReadFileContent("fileA.java")).Returns("FileContent");
      fileEnumeratorMock.Setup(f => f.ReadFileContent("fileB.java")).Returns(" File Content ");

      var sut = new DuplicateFileAnalyzer(new FilesystemService(fileEnumeratorMock.Object));

      var duplicateAnalysis = sut.ProcessAnalysis("fileA.java", filesToAnalyze);

      duplicateAnalysis.Should().Equal(new List<DuplicateFileAnalyzer.Similarity> { new("fileB.java", DuplicateFileAnalyzer.SimilarityMode.WhitespaceDifferences) });
    }

  }
}
