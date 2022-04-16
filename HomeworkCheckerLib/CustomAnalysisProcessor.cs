using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeworkCheckerLib
{
  internal class CustomAnalysisProcessor
  {
    private readonly FilesystemService filesystemService;

    public record CustomAnalysisResult(string CustomAnalysisOutput);

    public CustomAnalysisProcessor(FilesystemService filesystemService)
    {
      this.filesystemService = filesystemService;
    }

    public CustomAnalysisResult Process(string javaFile)
    {
      var sb = new StringBuilder();

      var fileContent = filesystemService.ReadFileContent(javaFile);
      using (StringReader reader = new(fileContent))
      {
        int lineNumber = 1;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
          AnalyzeLine(lineNumber, line, sb);
          ++lineNumber;
        }
      }

      return new(sb.ToString());
    }

    internal static void AnalyzeLine(int lineNumber, string line, StringBuilder sb)
    {
      sb.AppendLine($"[line {lineNumber}] TODO {line}");
    }
  }
}
