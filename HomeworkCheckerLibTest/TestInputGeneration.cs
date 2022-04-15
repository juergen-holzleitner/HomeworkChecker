using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestInputGeneration
  {
    [Fact]
    public void Can_get_inputs_from_folder()
    {
      var fileEnumeratorMock = new Mock<FilesystemService.IFileEnumerator>();
      fileEnumeratorMock.Setup(f => f.GetFilesInFolderRecursivly(It.IsAny<string>(), "*.txt")).Returns(new List<string> { "fileName.txt" });
      fileEnumeratorMock.Setup(f => f.ReadFileContent("fileName.txt")).Returns("fileContent");

      var sut = new InputGenerator(new FilesystemService(fileEnumeratorMock.Object));

      var input = sut.GetInputs(@"someFolder");

      input.Inputs.Should().Equal(new List<InputGenerator.Input> { new("fileName", "fileContent") });
    }


  }
}
