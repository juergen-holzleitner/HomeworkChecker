using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestJavaCompilation
  {
    [Fact]
    public void Can_call_java_compiler()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
               .Returns(new IAppExecuter.ExecutionResult(0, string.Empty));

      var sut = new JavaCompiler(appExecuterMock.Object);

      var result = sut.CompileFile(@"someFolder\someFileName.java");
      appExecuterMock.Verify(x => x.Execute("javac", "someFolder", "-Xlint \"someFileName.java\""), Times.Once());
    }

    [Fact]
    public void Returns_faild_when_javac_returns_exitcode()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
               .Returns(new IAppExecuter.ExecutionResult(-1, string.Empty));

      var sut = new JavaCompiler(appExecuterMock.Object);

      var result = sut.CompileFile(@"someFolder\someFileName.java");
      result.CompileSucceeded.Should().BeFalse();
    }
  }
}
