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
  public class TestPercentageAdder
  {
    [Fact]
    public void Can_run_PercentageAdder_on_folder()
    {
      var fileEnumerator = new Mock<FilesystemService.IFileEnumerator>();

      string folder = "homeworkFolder";
      fileEnumerator.Setup(f => f.GetFilesInFolderRecursivly(folder, It.IsAny<string>())).Returns(new List<string> { "fileA.java" });

      var sut = new PercentageAdder(fileEnumerator.Object);

      sut.ProcessPercentages(folder);

      fileEnumerator.Verify(f => f.GetFilesInFolderRecursivly(folder, "*.java"));
      fileEnumerator.Verify(f => f.ReadFileContent("fileA.java"));
    }

  }
}
