namespace HomeworkCheckerLib
{
  public class HomeworkChecker
  {
    public record MasterResult(string MasterFile);

    public HomeworkChecker()
      : this(new FileEnumerator())
    {
    }

    internal HomeworkChecker(DirectoryService.IFileEnumerator fileEnumerator)
    {
      directoryService = new DirectoryService(fileEnumerator);
    }

    public MasterResult ProcessMaster(string masterFolder)
    {
      var file = directoryService.GetAllJavaFiles(masterFolder).Single();
      return new(file);
    }

    readonly DirectoryService directoryService;
  }
}