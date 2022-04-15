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

      var sut = new JavaCompiler(appExecuterMock.Object);

      var result = sut.CompileFile(@"someFolder\someFileName.java");
      appExecuterMock.Verify(x => x.Execute("javac", "someFolder", "-Xlint \"someFileName.java\""), Times.Once());
    }
  }
}
