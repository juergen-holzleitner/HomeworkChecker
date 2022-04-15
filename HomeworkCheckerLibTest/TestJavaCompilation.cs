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
  public class TestJavaCompilation
  {
    [Fact]
    public void Can_call_java_compiler()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute("javac"));
      
      var sut = new JavaCompiler(appExecuterMock.Object);

      var result = sut.CompileFile(@"someFileName.java");
      appExecuterMock.Verify(x => x.Execute("javac"), Times.Once());
    }
  }
}
