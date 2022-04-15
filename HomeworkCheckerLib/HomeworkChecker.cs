namespace HomeworkCheckerLib
{
  public class HomeworkChecker
  {
    public record Input(string Filename, string FileContent);

    public record Output(Input Input, string OutputContent);

    public record MasterResult(string MasterFile, string CompileIssues, IEnumerable<Output> Outputs);

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
    }

    public MasterResult ProcessMaster(string masterFolder)
    {
      var file = directoryService.GetAllJavaFiles(masterFolder).Single();

      var compileResult = javaCompiler.CompileFile(file);
      if (!compileResult.CompileSucceeded)
      { 
        output.WriteError($"compiling {compileResult.JavaFile} failed");
        return new(file, compileResult.CompileOutput, new List<Output>());
      }
      output.WriteSuccess($"compiled {compileResult.JavaFile}");

      var outputs = GetProgramOutputs(compileResult.JavaFile, masterFolder);

      return new(file, string.Empty, outputs);
    }

    internal IEnumerable<Output> GetProgramOutputs(string fileName, string folder)
    {
      var inputs = inputGenerator.GetInputs(folder);
      foreach (var input in inputs.Inputs)
      {
        var output = outputGenerator.GenerateOutput(fileName, folder, input.FileContent);
        yield return new Output(input, output.Content);
      }
    }

    readonly FilesystemService directoryService;
    readonly JavaCompiler javaCompiler;
    readonly IRuntimeOutput output;
    readonly InputGenerator inputGenerator;
    readonly OutputGenerator outputGenerator;
  }
}