using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeworkCheckerLib
{
  public class PercentageAdder
  {
    public PercentageAdder()
      : this(new FileEnumerator())
    { }
    private readonly FilesystemService filesystemService;

    internal PercentageAdder(FilesystemService.IFileEnumerator fileEnumerator)
    {
      filesystemService = new (fileEnumerator);
    }
    public void ProcessPercentages(string folder)
    {
      var files = filesystemService.GetAllJavaFiles(folder);
      foreach (var file in files)
        ProcessFile(file);
    }

    private void ProcessFile(string file)
    {
      var fileContent = filesystemService.ReadFileContent(file);
    }
  }
}
