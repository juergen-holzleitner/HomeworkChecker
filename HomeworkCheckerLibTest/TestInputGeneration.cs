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
  public class TestInputGeneration
  {
    [Fact]
    public void Can_get_inputs_from_folder()
    {
      var fileEnumeratorMock = new Mock<DirectoryService.IFileEnumerator>();
      fileEnumeratorMock.Setup(f => f.GetFilesInFolderRecursivly(It.IsAny<string>(), "*.txt")).Returns(new List<string> { "fileName.txt" });

      var sut = new InputGenerator(new DirectoryService(fileEnumeratorMock.Object));

      var input = sut.GetInputs(@"someFolder");

      input.Inputs.Should().Equal(new List<InputGenerator.Input> { new("fileName") });
    }

    
  }
}
