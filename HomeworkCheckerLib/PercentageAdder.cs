using System.Text.RegularExpressions;

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
      filesystemService = new(fileEnumerator);
      this.output = output;
    }
    public void ProcessPercentages(string folder)
    {
      var files = filesystemService.GetAllJavaFiles(folder);
      foreach (var file in files)
        ProcessFile(folder, file);
    }

    private void ProcessFile(string folder, string fileName)
    {
      int totalPercentage = 100;
      int? totalPercentageLine = null;
      int? previousTotalPercentageValue = null;
      int lastNonEmptyLine = 0;
      var outputName = fileName[folder.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

      var fileContent = filesystemService.ReadFileContent(fileName);
      using (var reader = new StringReader(fileContent))
      {
        int lineNumber = 1;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
          GeneralLineCheck(outputName, lineNumber, line);

          var linePercentage = GetPercentageFromLine(outputName, lineNumber, line) ?? 0;
          totalPercentage += linePercentage;

          var percentageValue = GetTotalPercentageValue(line);
          if (percentageValue.HasValue)
          {
            totalPercentageLine = lineNumber;
            previousTotalPercentageValue = percentageValue;
          }

          if (!string.IsNullOrEmpty(line))
            lastNonEmptyLine = lineNumber;

          ++lineNumber;
        }
      }

      if (totalPercentage <= 0)
        output.WriteWarning($"{outputName}: invalid final percentage {totalPercentage}%");

      var totalPercentageText = $"// [Total: {totalPercentage}%]{Environment.NewLine}";
      if (totalPercentageLine.HasValue)
      {
        if (previousTotalPercentageValue!.Value != totalPercentage)
        {
          ReplaceText(fileName, totalPercentageLine.Value, totalPercentageText);
          output.WriteSuccess($"{outputName}: (updated from {previousTotalPercentageValue!.Value}%): {totalPercentage}%");
        }
        else
          output.WriteSuccess($"{outputName}: (already set): {totalPercentage}%");
      }
      else
      {
        AddText(fileName, lastNonEmptyLine, totalPercentageText);
        output.WriteSuccess($"{outputName}: {totalPercentage}%");
      }
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

    internal int? GetPercentageFromLine(string outputName, int lineNumber, string line)
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
            output.WriteWarning($"{outputName}:{lineNumber}: unusual percentage {valueText}%");
          totalValue += value;
        }
      }

      return totalValue;
    }

    internal void GeneralLineCheck(string outputName, int lineNumber, string line)
    {
      int pos = line.ToUpper().IndexOf("TODO");
      if (pos >= 0)
        output.WriteWarning($"{outputName}:{lineNumber}: {line[pos..]}");
    }

    internal static int? GetTotalPercentageValue(string line)
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
