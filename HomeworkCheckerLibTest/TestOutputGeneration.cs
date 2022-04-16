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
      appExecuterMock.Setup(a => a.Execute("java", "workingDir", "fileName", "inputContent", 5000))
                     .Returns(new IAppExecuter.ExecutionResult(0, "output content", false));
      var sut = new OutputGenerator(appExecuterMock.Object);

      var output = sut.GenerateOutput("fileName.java", "workingDir", "inputContent");

      output.Should().Be(new OutputGenerator.Output("output content", false));
    }

    [Fact]
    public void Can_handle_timeout()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(a => a.Execute("java", "workingDir", "fileName", "inputContent", 5000))
                     .Returns(new IAppExecuter.ExecutionResult(0, "output content", true));
      var sut = new OutputGenerator(appExecuterMock.Object);

      var output = sut.GenerateOutput("fileName.java", "workingDir", "inputContent");

      output.Should().Be(new OutputGenerator.Output("output content", true));
    }

  }
}
