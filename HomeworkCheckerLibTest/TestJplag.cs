using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestJplag
  {
    [Fact]
    public void Can_process_jplag()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.ExecuteAsciiOutput("java", Path.Combine("currentFolder", "jplag"), "-jar jplag-4.0.0-SNAPSHOT-jar-with-dependencies.jar -m 100 -t 4 -r \"TEMP-jplag-result\" \"masterFolder\" \"homeworkFolder\""))
        .Returns(new IAppExecuter.ExecutionResult(0, "jplag result", false));

      var sut = new JplagProcessor(appExecuterMock.Object, Mock.Of<FilesystemService.IFileEnumerator>());

      var result = sut.Process("masterFolder", "homeworkFolder");

      result.Similarities.Should().BeEmpty();
    }

    [Fact]
    public void Jplag_temp_folder_is_finally_removed()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.ExecuteAsciiOutput("java", Path.Combine("currentFolder", "jplag"), "-jar jplag-4.0.0-SNAPSHOT-jar-with-dependencies.jar -m 100 -t 4 -r \"TEMP-jplag-result\" \"masterFolder\" \"homeworkFolder\""))
        .Returns(new IAppExecuter.ExecutionResult(0, "jplag result", false));

      var fileSystemMock = new Mock<FilesystemService.IFileEnumerator>();

      var sut = new JplagProcessor(appExecuterMock.Object, fileSystemMock.Object);

      sut.Process("masterFolder", "homeworkFolder");

      fileSystemMock.Verify(f => f.RemoveFolderIfExists(Path.Combine("currentFolder", "jplag", "TEMP-jplag-result")));
    }

    [Fact]
    public void Can_get_jplag_similarities()
    {
      var jplagOutput = "Comparing \"file³\" - \"fileB\": 50.23";

      var result = JplagProcessor.GetSimilarities(jplagOutput);

      result.Should().Equal(new List<JplagProcessor.JplagSimilarity> { new("fileü", "fileB", 50.23) });
    }

    [Theory]
    [InlineData(3, 3)]
    [InlineData(2, 1)]
    [InlineData(4, 6)]
    public void Can_get_num_expected_similarities(int numberOfFiles, int expectedNumberOfSimilarities)
    {
      var numberOfSimilarities = JplagProcessor.GetExpectedNumberOfSimilarities(numberOfFiles);

      numberOfSimilarities.Should().Be(expectedNumberOfSimilarities);
    }

    [Fact]
    public void Can_get_submission_similarities()
    {
      var similarities = new List<JplagProcessor.JplagSimilarity> { new("fileA", "fileB", 99.9) };

      var result = JplagProcessor.GetSubmissionSimilarities("fileA", similarities);
      result.Should().Equal(new List<JplagProcessor.SubmissionSimilarity> { new("fileB", 99.9) });

      result = JplagProcessor.GetSubmissionSimilarities("fileB", similarities);
      result.Should().Equal(new List<JplagProcessor.SubmissionSimilarity> { new("fileA", 99.9) });
    }
  }
}
