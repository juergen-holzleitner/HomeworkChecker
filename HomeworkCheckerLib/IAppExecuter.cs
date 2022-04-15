using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeworkCheckerLib
{
  internal interface IAppExecuter
  {
    void Execute(string appName, string workingDirectory, string arguments);
  }
}
