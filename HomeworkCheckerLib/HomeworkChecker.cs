namespace HomeworkCheckerLib
{
  public class HomeworkChecker
  {
    public record CompileIssues(IEnumerable<string> Issues);

    public record MasterResult(string MasterFile, CompileIssues CompileIssues);

    public HomeworkChecker()
      : this(new FileEnumerator(), new AppExecuter(), new RuntimeOutput())
    {
    }

    internal HomeworkChecker(DirectoryService.IFileEnumerator fileEnumerator, IAppExecuter appExecuter, IRuntimeOutput output)
    {
      directoryService = new DirectoryService(fileEnumerator);
      javaCompiler = new JavaCompiler(appExecuter);
      this.output = output;
    }

    public MasterResult ProcessMaster(string masterFolder)
    {
      var file = directoryService.GetAllJavaFiles(masterFolder).Single();

      var compileResult = javaCompiler.CompileFile(file);
      if (compileResult.CompileSucceeded)
        output.WriteSuccess($"compiled {compileResult.JavaFile}");
      else
        output.WriteError($"compiling {compileResult.JavaFile} failed");

      return new(file, new(new List<string>()));
    }

    readonly DirectoryService directoryService;
    readonly JavaCompiler javaCompiler;
    private readonly IRuntimeOutput output;
  }
}