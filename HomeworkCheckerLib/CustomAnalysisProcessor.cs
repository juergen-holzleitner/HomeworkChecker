using System.Text;
using System.Text.RegularExpressions;

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
      var checks = new List<(string regEx, string errorMessage)>()
      {
        new (@"format[^%]*\)", "use of format without %"),
        new ("printf", "use of printf instead of format"),
        new ("\"\"", "use of empty string"),
        new ("\".\"", "use of string for a single character"),
        new (@"\s+[%\\]n", "space before newline"),
        new (@"\\n", "use of \\n instead of %n"),
        new (@"format\s*\(\s*""\s*[%\\]n\s*""\s*\)", "format is used to print a newline"),

      };

      foreach (var check in checks)
      {
        var regEx = new Regex(check.regEx);
        if (regEx.IsMatch(line))
          sb.AppendLine($"{lineNumber}: " + check.errorMessage);
      }
    }
  }
}
