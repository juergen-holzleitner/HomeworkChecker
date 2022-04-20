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

    internal void AddText(string fileName, int lineNumber, string textToAdd)
    {
      var fileContent = filesystemService.ReadFileContent(fileName);
      using StringReader reader = new(fileContent);
      using StringWriter writer = new();
      int currentLine = 1;
      string? line;
      while ((line = reader.ReadLine()) != null)
      {
        writer.WriteLine(line);
        if (lineNumber == currentLine)
          writer.Write(textToAdd);
        ++currentLine;
      }
      filesystemService.WriteFileContent(fileName, writer.ToString());
    }

    internal void ReplaceText(string fileName, int lineNumber, string textToAdd)
    {
      var fileContent = filesystemService.ReadFileContent(fileName);
      using StringReader reader = new(fileContent);
      using StringWriter writer = new();
      int currentLine = 1;
      string? line;
      while ((line = reader.ReadLine()) != null)
      {
        if (lineNumber == currentLine)
          writer.Write(textToAdd);
        else
          writer.WriteLine(line);
        ++currentLine;
      }
      filesystemService.WriteFileContent(fileName, writer.ToString());
    }
  }
}
