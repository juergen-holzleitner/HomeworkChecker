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
               .Returns(new IAppExecuter.ExecutionResult(0, string.Empty, false));

      var sut = new JavaCompiler(appExecuterMock.Object);

      var result = sut.CompileFile(@"someFolder\someFileName.java");
      appExecuterMock.Verify(x => x.Execute("javac", "someFolder", "-Xlint -encoding UTF8 \"someFileName.java\""), Times.Once());
    }

    [Fact]
    public void Returns_faild_when_javac_returns_exitcode()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
               .Returns(new IAppExecuter.ExecutionResult(-1, "someFileName.java:4: error: <identifier> expected", false));

      var sut = new JavaCompiler(appExecuterMock.Object);

      var result = sut.CompileFile(@"someFolder\someFileName.java");
      result.CompileSucceeded.Should().BeFalse();
      result.CompileOutput.Should().Be("someFileName.java:4: error: <identifier> expected");
    }
  }
}
