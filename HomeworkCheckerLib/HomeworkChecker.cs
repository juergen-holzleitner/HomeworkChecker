namespace HomeworkCheckerLib
{
  public class HomeworkChecker
  {
    public record Input(string Filename, string FileContent);

    public record Output(Input Input, string OutputContent, bool HasTimedOut);

    public record MasterResult(string MasterFile, string CompileIssues, IEnumerable<Output> Outputs, string CheckstyleIssues, string PMDIssues, string SpotBugsIssues, string CustomAnalysisIssues);

    public HomeworkChecker()
      : this(new FileEnumerator(), new AppExecuter(), new RuntimeOutput())
    {
    }

    internal HomeworkChecker(FilesystemService.IFileEnumerator fileEnumerator, IAppExecuter appExecuter, IRuntimeOutput output)
    {
      filesystemService = new FilesystemService(fileEnumerator);
      javaCompiler = new JavaCompiler(appExecuter);
      this.output = output;
      inputGenerator = new InputGenerator(filesystemService);
      outputGenerator = new OutputGenerator(appExecuter);
      checkstyleProcessor = new CheckstyleProcessor(appExecuter);
      pmdProcessor = new PMDProcessor(appExecuter);
      spotBugsProcessor = new SpotBugsProcessor(appExecuter);
      customAnalysisProcessor = new CustomAnalysisProcessor(filesystemService);
    }

    public MasterResult ProcessMaster(string masterFolder)
    {
      output.WriteInfo($"processing {masterFolder}");

      var javaFile = filesystemService.GetAllJavaFiles(masterFolder).Single();

      var compileOutput = string.Empty;
      IEnumerable<Output> outputs = Enumerable.Empty<Output>();

      var compileResult = javaCompiler.CompileFile(javaFile);
      if (!compileResult.CompileSucceeded)
      {
        output.WriteError($"compiling {compileResult.JavaFile} failed");
        compileOutput = compileResult.CompileOutput;
      }
      else
      {
        output.WriteSuccess($"compiled {compileResult.JavaFile}");

        outputs = GetProgramOutputs(compileResult.JavaFile, masterFolder);
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

      return new(javaFile, compileOutput, outputs, checkstyleResult.CheckstyleOutput, pmdResult.PMDOutput, spotBugsOutput, customAnalysisResult.CustomAnalysisOutput);
    }

    internal IEnumerable<Output> GetProgramOutputs(string fileName, string folder)
    {
      var inputData = inputGenerator.GetInputs(folder);
      return GetProgramOutputs(fileName, folder, inputData);
    }

    internal IEnumerable<Output> GetProgramOutputs(string fileName, string folder, InputGenerator.InputData inputData)
    {
      if (inputData.Inputs.Any())
      {
        foreach (var input in inputData.Inputs)
        {
          var output = outputGenerator.GenerateOutput(fileName, folder, input.FileContent);
          yield return new Output(input, output.Content, output.HasTimedOut);
        }
      }
      else
      {
        var output = outputGenerator.GenerateOutput(fileName, folder, null);
        yield return new Output(new("<no input>", string.Empty), output.Content, output.HasTimedOut);
      }
    }

    readonly FilesystemService filesystemService;
    readonly JavaCompiler javaCompiler;
    readonly IRuntimeOutput output;
    readonly InputGenerator inputGenerator;
    readonly OutputGenerator outputGenerator;
    readonly CustomAnalysisProcessor customAnalysisProcessor;
    readonly CheckstyleProcessor checkstyleProcessor;
    readonly PMDProcessor pmdProcessor;
    readonly SpotBugsProcessor spotBugsProcessor;
  }
}