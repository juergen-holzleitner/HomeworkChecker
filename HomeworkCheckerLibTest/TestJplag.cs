using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "jplag"), "-jar jplag-4.0.0-SNAPSHOT-jar-with-dependencies.jar -m 100 -t 4 -r \"TEMP-jplag-result\" \"masterFolder\" \"homeworkFolder\""))
        .Returns(new IAppExecuter.ExecutionResult(0, "jplag result", false));

      var sut = new JplagProcessor(appExecuterMock.Object, Mock.Of<FilesystemService.IFileEnumerator>());

      var result = sut.Process("masterFolder", "homeworkFolder");

      result.Should().Be(new JplagProcessor.JplagResult("jplag result"));
    }

    [Fact]
    public void Jplag_temp_folder_is_finally_removed()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "jplag"), "-jar jplag-4.0.0-SNAPSHOT-jar-with-dependencies.jar -m 100 -t 4 -r \"TEMP-jplag-result\" \"masterFolder\" \"homeworkFolder\""))
        .Returns(new IAppExecuter.ExecutionResult(0, "jplag result", false));

      var fileSystemMock = new Mock<FilesystemService.IFileEnumerator>();

      var sut = new JplagProcessor(appExecuterMock.Object, fileSystemMock.Object);

      sut.Process("masterFolder", "homeworkFolder");

      fileSystemMock.Verify(f => f.RemoveFolderIfExists(Path.Combine("currentFolder", "jplag", "TEMP-jplag-result")));
    }
  }
}
