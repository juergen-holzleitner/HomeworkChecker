using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeworkCheckerLib
{
  internal class JavaCompiler
  {
    private readonly IAppExecuter appExecuter;

    public JavaCompiler(IAppExecuter appExecuter)
    {
      this.appExecuter = appExecuter;
    }

    internal object CompileFile(string fileName)
    {
      appExecuter.Execute("javac");
      return null;
    }
  }
}
