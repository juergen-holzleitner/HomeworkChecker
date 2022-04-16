namespace HomeworkCheckerLib
{
  public class HomeworkChecker
  {
    public record Input(string Filename, string FileContent);

    public record Output(Input Input, string OutputContent, bool HasTimedOut);

    public record MasterResult(string MasterFile, string CompileIssues, IEnumerable<Output> Outputs, string CheckstyleIssues);

    public HomeworkChecker()
      : this(new FileEnumerator(), new AppExecuter(), new RuntimeOutput())
    {
    }

    internal HomeworkChecker(FilesystemService.IFileEnumerator fileEnumerator, IAppExecuter appExecuter, IRuntimeOutput output)
    {
      directoryService = new FilesystemService(fileEnumerator);
      javaCompiler = new JavaCompiler(appExecuter);
      this.output = output;
      inputGenerator = new InputGenerator(directoryService);
      outputGenerator = new OutputGenerator(appExecuter);
      checkstyleProcessor = new CheckstyleProcessor(appExecuter);
    }

    public MasterResult ProcessMaster(string masterFolder)
    {
      output.WriteInfo($"processing {masterFolder}");

      var file = directoryService.GetAllJavaFiles(masterFolder).Single();

      var checkstyleResult = checkstyleProcessor.Process(file);
      if (checkstyleResult.ExitCode != 0 || !string.IsNullOrEmpty(checkstyleResult.CheckstyleOutput))
        output.WriteWarning("checkstyle issues");
      else
        output.WriteSuccess("checkstyle processed");

      var compileResult = javaCompiler.CompileFile(file);
      if (!compileResult.CompileSucceeded)
      {
        output.WriteError($"compiling {compileResult.JavaFile} failed");
        return new(file, compileResult.CompileOutput, new List<Output>(), checkstyleResult.CheckstyleOutput);
      }
      output.WriteSuccess($"compiled {compileResult.JavaFile}");

      var outputs = GetProgramOutputs(compileResult.JavaFile, masterFolder);
      foreach (var programOutput in outputs)
      {
        if (programOutput.HasTimedOut)
          output.WriteError($"generation of output for {programOutput.Input.Filename} timed out");
        else
          output.WriteSuccess($"generated output for {programOutput.Input.Filename}");
      }
      return new(file, string.Empty, outputs, checkstyleResult.CheckstyleOutput);
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

    readonly FilesystemService directoryService;
    readonly JavaCompiler javaCompiler;
    readonly IRuntimeOutput output;
    readonly InputGenerator inputGenerator;
    readonly OutputGenerator outputGenerator;
    readonly CheckstyleProcessor checkstyleProcessor;
  }
}