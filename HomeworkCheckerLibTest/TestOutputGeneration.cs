using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestOutputGeneration
  {
    [Fact]
    public void Can_generate_output()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(a => a.Execute("java", "workingDir", "fileName", "inputContent"))
                     .Returns(new IAppExecuter.ExecutionResult(0, "output content"));
      var sut = new OutputGenerator(appExecuterMock.Object);

      var output = sut.GenerateOutput("fileName.java", "workingDir", "inputContent");

      output.Should().Be(new OutputGenerator.Output("output content"));

    }
  }
}
