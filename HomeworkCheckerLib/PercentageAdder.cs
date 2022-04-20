using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HomeworkCheckerLib
{
  public class PercentageAdder
  {
    public PercentageAdder()
      : this(new FileEnumerator(), new RuntimeOutput())
    { }
    private readonly FilesystemService filesystemService;
    private readonly IRuntimeOutput output;

    internal PercentageAdder(FilesystemService.IFileEnumerator fileEnumerator, IRuntimeOutput output)
    {
      filesystemService = new (fileEnumerator);
      this.output = output;
    }
    public void ProcessPercentages(string folder)
    {
      var files = filesystemService.GetAllJavaFiles(folder);
      foreach (var file in files)
        ProcessFile(file);
    }

    private void ProcessFile(string fileName)
    {
      int totalPercentage = 100;
      int? totalPercentageLine = null;
      int lastNonEmptyLine = 0;

      var fileContent = filesystemService.ReadFileContent(fileName);
      using (var reader = new StringReader(fileContent))
      {
        int lineNumber = 1;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
          GeneralLineCheck(lineNumber, line);

          var linePercentage = GetPercentageFromLine(lineNumber, line) ?? 0;
          totalPercentage += linePercentage;

          if (GetTotalPercentageValue(line).HasValue)
            totalPercentageLine = lineNumber;

          if (!string.IsNullOrEmpty(line))
            lastNonEmptyLine = lineNumber;

          ++lineNumber;
        }
      }

      var totalPercentageText = $"// [Total: {totalPercentage}%]{Environment.NewLine}";
      if (totalPercentageLine.HasValue)
        ReplaceText(fileName, totalPercentageLine.Value, totalPercentageText);
      else
        AddText(fileName, lastNonEmptyLine, totalPercentageText);
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

    internal int? GetPercentageFromLine(int lineNumber, string line)
    {
      var precentagePattern = new Regex(@".*?//.*?\[\s*(?<percentage>[+-]?\d+)\s*%\s*\]");
      var matches = precentagePattern.Matches(line);
      if (!matches.Any())
        return null;

      int totalValue = 0;
      foreach (var m in matches)
      {
        if (m is Match match)
        {
          var valueText = match.Groups["percentage"].Value;
          var value = int.Parse(valueText);
          if (value >= 0)
            output.WriteWarning($"line {lineNumber}: unusual percentage {valueText}%");
          totalValue += value;
        }
      }

      return totalValue;
    }

    internal void GeneralLineCheck(int lineNumber, string line)
    {
      int pos = line.ToUpper().IndexOf("TODO");
      if (pos >= 0)
        output.WriteWarning($"line {lineNumber}: {line[pos..]}");
    }

    internal int? GetTotalPercentageValue(string line)
    {
      var totalPattern = new Regex(@".*?//.*?\[\s*Total:\s*(?<percentage>[+-]?\d+)\s*%\s*\]");
      var match = totalPattern.Match(line);
      if (!match.Success)
        return null;

      var valueText = match.Groups["percentage"].Value;
      var value = int.Parse(valueText);
      return value;
    }
  }
}
