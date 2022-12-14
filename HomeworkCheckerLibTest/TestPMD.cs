using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.IO;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestPMD
  {
    [Fact]
    public void Can_process_PMD()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.Execute("cmd.exe", Path.Combine("currentFolder", "pmd", "bin"), "/c pmd.bat -d \"file.java\" -R Rules.xml -f text --short-names --no-cache -language java"))
        .Returns(new IAppExecuter.ExecutionResult(0, "PMD result", false));

      var sut = new PMDProcessor(appExecuterMock.Object);

      var result = sut.Process("file.java");

      result.Should().Be(new PMDProcessor.PMDResult(0, "PMD result"));
    }
  }
}
