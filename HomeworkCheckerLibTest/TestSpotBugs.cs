using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.IO;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestSpotBugs
  {
    [Fact]
    public void Can_process_SpotBugs()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "spotbugs", "lib"), "-jar spotbugs.jar -textui -effort:max -low -longBugCodes -dontCombineWarnings -exclude ..\\exclude.xml \"file.class\""))
        .Returns(new IAppExecuter.ExecutionResult(0, "spotbugs result", false));

      var sut = new SpotBugsProcessor(appExecuterMock.Object);

      var result = sut.Process("file.java");

      result.Should().Be(new SpotBugsProcessor.SpotBugsResult(0, "spotbugs result"));
    }
  }
}
