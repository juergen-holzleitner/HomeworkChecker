namespace HomeworkCheckerLib
{
  public class HomeworkChecker
  {
    public record Input(string Filename, string FileContent);

    public record Output(Input Input, string OutputContent, bool HasTimedOut);

    public record FileAnalysisResult(string FileName, string CompileIssues, IEnumerable<Output> Outputs, string CheckstyleIssues, string PMDIssues, string SpotBugsIssues, string CustomAnalysisIssues);

    public record SimilarityAnalysis(IEnumerable<DuplicateFileAnalyzer.Similarity> Duplicates, IEnumerable<JplagProcessor.SubmissionSimilarity> JplagSimilarities, JplagProcessor.SubmissionSimilarity? JplagMasterSimilarity);

    public record FileNameAnalysis(string Name, string ExpectedName, TextDiffGenerator.Difference FileNameDifference);

    public record SubmissionAnalysis(SimilarityAnalysis Similarities, FileNameAnalysis FileNameAnalysis, OutputDifferencesAnalyzer.OutputDifferenceAnalysis OutputDifference, FileAnalysisResult AnalysisResult);

    public record HomeworkResult(FileAnalysisResult MasterResult, IEnumerable<SubmissionAnalysis> Submissions);

    public HomeworkChecker()
      : this(new FileEnumerator(), new AppExecuter(), new RuntimeOutput())
    {
    }

    internal HomeworkChecker(FilesystemService.IFileEnumerator fileEnumerator, IAppExecuter appExecuter, IRuntimeOutput output)
    {
      filesystemService = new FilesystemService(fileEnumerator);
      javaCompiler = new JavaCompiler(appExecuter);
      this.appExecuter = appExecuter;
      this.output = output;
      inputGenerator = new InputGenerator(filesystemService);
      outputGenerator = new OutputGenerator(appExecuter);
      checkstyleProcessor = new CheckstyleProcessor(appExecuter);
      pmdProcessor = new PMDProcessor(appExecuter);
      spotBugsProcessor = new SpotBugsProcessor(appExecuter);
      customAnalysisProcessor = new CustomAnalysisProcessor(filesystemService);
      jplagProcessor = new JplagProcessor(appExecuter, fileEnumerator);
      duplicateFileAnalyzer = new DuplicateFileAnalyzer(filesystemService);
    }

    public FileAnalysisResult ProcessMaster(string masterFolder)
    {
      var inputData = inputGenerator.GetInputs(masterFolder);

      return ProcessMaster(masterFolder, inputData);
    }

    private FileAnalysisResult ProcessMaster(string masterFolder, InputGenerator.InputData inputData)
    {
      var javaFile = filesystemService.GetAllJavaFiles(masterFolder).Single();
      var outputName = javaFile[masterFolder.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      return ProcessFileAnalysis(outputName, javaFile, inputData);
    }

    private FileAnalysisResult ProcessFileAnalysis(string outputName, string javaFile, InputGenerator.InputData inputData)
    {
      output.WriteInfo($"processing {outputName}");

      var compileOutput = string.Empty;
      IEnumerable<Output> outputs = Enumerable.Empty<Output>();

      var compileResult = javaCompiler.CompileFile(javaFile);
      if (!compileResult.CompileSucceeded)
      {
        output.WriteError($"compiling {outputName} failed");
        compileOutput = compileResult.CompileOutput;
      }
      else
      {
        output.WriteSuccess($"compiled");

        outputs = GetProgramOutputs(javaFile, inputData);
        foreach (var programOutput in outputs)
        {
          if (programOutput.HasTimedOut)
            output.WriteError($"generation of output for {programOutput.Input.Filename} timed out");
          else
            output.WriteSuccess($"generated output for {programOutput.Input.Filename}");
        }
      }

      var customAnalysisResult = customAnalysisProcessor.Process(javaFile);
      if (!string.IsNullOrEmpty(customAnalysisResult.CustomAnalysisOutput))
        output.WriteWarning("custom analysis issues");
      else
        output.WriteSuccess("custom analysis processed");

      var checkstyleResult = checkstyleProcessor.Process(javaFile);
      if (checkstyleResult.ExitCode != 0 || !string.IsNullOrEmpty(checkstyleResult.CheckstyleOutput))
        output.WriteWarning("checkstyle issues");
      else
        output.WriteSuccess("checkstyle processed");

      var pmdResult = pmdProcessor.Process(javaFile);
      if (pmdResult.ExitCode != 0 || !string.IsNullOrEmpty(pmdResult.PMDOutput))
        output.WriteWarning("PMD issues");
      else
        output.WriteSuccess("PMD processed");

      var spotBugsOutput = string.Empty;
      if (compileResult.CompileSucceeded)
      {
        var spotBugsResult = spotBugsProcessor.Process(javaFile);
        if (spotBugsResult.ExitCode != 0)
          output.WriteError("SpotBugs failed");
        else if (!string.IsNullOrEmpty(spotBugsResult.SpotBugsOutput))
          output.WriteWarning("SpotBugs issues");
        else
          output.WriteSuccess("SpotBugs processed");
        spotBugsOutput = spotBugsResult.SpotBugsOutput;
      }

      var classFile = Path.ChangeExtension(javaFile, "class");
      filesystemService.RemoveFileIfExists(classFile);

      return new(javaFile, compileOutput, outputs, checkstyleResult.CheckstyleOutput, pmdResult.PMDOutput, spotBugsOutput, customAnalysisResult.CustomAnalysisOutput);
    }

    public HomeworkResult ProcessHomework(string masterFolder, string homeworkFolder)
    {
      var inputData = inputGenerator.GetInputs(masterFolder);

      var masterResult = ProcessMaster(masterFolder, inputData);
      var masterFileName = Path.GetFileName(masterResult.FileName);

      var homeworkFiles = filesystemService.GetAllJavaFiles(homeworkFolder);

      output.WriteInfo(Environment.NewLine);

      var jplagResult = jplagProcessor.Process(masterFolder, homeworkFolder);
      var numResults = jplagResult.Similarities.Count();
      const int numberOfMasterFiles = 1;
      int numberOfHomeworkFiles = homeworkFiles.Count();
      var numResultsExpected = JplagProcessor.GetExpectedNumberOfSimilarities(numberOfHomeworkFiles + numberOfMasterFiles);
      if (numResults != numResultsExpected)
        output.WriteWarning($"processed jplag with {numResults} result(s), but {numResultsExpected} were expected");
      else
        output.WriteSuccess($"processed jplag with {numResults} result(s)");

      var possibleDuplicateFiles = Enumerable.Append(homeworkFiles, masterResult.FileName);

      List<SubmissionAnalysis> submissions = new();
      foreach (var homeworkFile in homeworkFiles)
      {
        output.WriteInfo(Environment.NewLine);

        var outputName = homeworkFile[homeworkFolder.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var analysisResult = ProcessFileAnalysis(outputName, homeworkFile, inputData);

        var duplicateInfo = duplicateFileAnalyzer.ProcessAnalysis(homeworkFile, possibleDuplicateFiles);
        var jplagSimilarities = JplagProcessor.GetSubmissionSimilarities(homeworkFile, jplagResult.Similarities);
        var masterSimilarity = jplagSimilarities.Where(j => j.File == masterResult.FileName).SingleOrDefault();
        var similarityAnalysis = new SimilarityAnalysis(duplicateInfo, jplagSimilarities.Where(j => j.File != masterResult.FileName), masterSimilarity);
        if (duplicateInfo.Any())
          output.WriteWarning($"{homeworkFile} has {duplicateInfo.Count()} duplicate(s)");

        string homeworkFileName = Path.GetFileName(homeworkFile);
        var fileNameDifferences = TextDiffGenerator.GenerateDiff(masterFileName, homeworkFileName);
        if (fileNameDifferences.Diffs.Any())
          output.WriteWarning($"{homeworkFile} has filename differences");
        var fileNameAnalysis = new FileNameAnalysis(masterFileName, homeworkFileName, fileNameDifferences);

        var outputAnalysis = OutputDifferencesAnalyzer.GetDifferences(masterResult.Outputs, analysisResult.Outputs);
        var numDifferences = outputAnalysis.Differences.Count(d => d.DifferenceType != OutputDifferencesAnalyzer.DifferenceType.Equal);
        if (numDifferences > 0)
          output.WriteWarning($"{numDifferences} output(s) differ");

        submissions.Add(new(similarityAnalysis, fileNameAnalysis, outputAnalysis, analysisResult));
      }

      return new(masterResult, submissions);
    }

    public void WriteAnalysisToMarkdownFile(FileAnalysisResult analysisResult)
    {
      var markdownFile = Path.Combine(Path.GetDirectoryName(analysisResult.FileName)!, "NOTES.md");
      var markdownText = MarkdownGenerator.FromFileAnalysis(analysisResult);
      filesystemService.AppendMarkdown(markdownFile, markdownText);
    }

    public void WriteAnalysisToMarkdownFile(HomeworkResult analysisResult)
    {
      foreach (var analysis in analysisResult.Submissions)
      {
        var markdownFile = Path.Combine(Path.GetDirectoryName(analysis.AnalysisResult.FileName)!, "NOTES.md");
        var markdownText = MarkdownGenerator.FromSubmissionAnalysis(analysis);
        filesystemService.AppendMarkdown(markdownFile, markdownText);
      }
    }

    public int StartVSCodeWithFolder(string folder)
    {
      output.WriteInfo(Environment.NewLine);
      output.WriteInfo("waiting for VS code to close ...");

      var result = appExecuter.Execute("cmd.exe", folder, "/c code.cmd --wait .");
      return result.ExitCode;
    }

    public void CleanUpMarkdownFiles(FileAnalysisResult fileAnalysisResult)
    {
      RemoveMarkdownFile(fileAnalysisResult);
      output.WriteInfo("NOTES.md cleared");
    }

    private void RemoveMarkdownFile(FileAnalysisResult fileAnalysisResult)
    {
      var path = Path.GetDirectoryName(fileAnalysisResult.FileName)!;
      filesystemService.RemoveFileIfExists(Path.Combine(path, "NOTES.md"));
    }

    public void CleanUpMarkdownFiles(HomeworkResult homeworkResult)
    {
      foreach (var submission in homeworkResult.Submissions)
      {
        var path = Path.GetDirectoryName(submission.AnalysisResult.FileName)!;
        filesystemService.RemoveFileIfExists(Path.Combine(path, "NOTES.md"));
      }
      output.WriteInfo("NOTES.md cleared");
    }

    internal IEnumerable<Output> GetProgramOutputs(string fileName, InputGenerator.InputData inputData)
    {
      if (inputData.Inputs.Any())
      {
        foreach (var input in inputData.Inputs)
        {
          var output = outputGenerator.GenerateOutput(fileName, input.FileContent);
          yield return new Output(input, output.Content, output.HasTimedOut);
        }
      }
      else
      {
        var output = outputGenerator.GenerateOutput(fileName, null);
        yield return new Output(new("<no input>", string.Empty), output.Content, output.HasTimedOut);
      }
    }

    readonly FilesystemService filesystemService;
    readonly JavaCompiler javaCompiler;
    private readonly IAppExecuter appExecuter;
    readonly IRuntimeOutput output;
    readonly InputGenerator inputGenerator;
    readonly OutputGenerator outputGenerator;
    readonly CustomAnalysisProcessor customAnalysisProcessor;
    readonly CheckstyleProcessor checkstyleProcessor;
    readonly PMDProcessor pmdProcessor;
    readonly SpotBugsProcessor spotBugsProcessor;
    readonly JplagProcessor jplagProcessor;
    readonly DuplicateFileAnalyzer duplicateFileAnalyzer;
  }
}