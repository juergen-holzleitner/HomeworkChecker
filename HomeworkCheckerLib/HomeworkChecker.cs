namespace HomeworkCheckerLib
{
  public class HomeworkChecker
  {
    public record CompileIssues(IList<string> Issues);

    public record MasterResult(string MasterFile, CompileIssues CompileIssues);

    public HomeworkChecker()
      : this(new FileEnumerator(), new AppExecuter())
    {
    }

    internal HomeworkChecker(DirectoryService.IFileEnumerator fileEnumerator, IAppExecuter appExecuter)
    {
      directoryService = new DirectoryService(fileEnumerator);
      javaCompiler = new JavaCompiler(appExecuter);
    }

    public MasterResult ProcessMaster(string masterFolder)
    {
      var file = directoryService.GetAllJavaFiles(masterFolder).Single();

      javaCompiler.CompileFile(file);

      return new(file, new(new List<string>()));
    }

    readonly DirectoryService directoryService;
    readonly JavaCompiler javaCompiler;
  }
}