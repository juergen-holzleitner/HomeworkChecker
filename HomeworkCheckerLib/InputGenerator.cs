namespace HomeworkCheckerLib
{
  internal class InputGenerator
  {
    private readonly FilesystemService directoryService;

    internal record InputData(IEnumerable<HomeworkChecker.Input> Inputs);

    public InputGenerator(FilesystemService directoryService)
    {
      this.directoryService = directoryService;
    }

    internal InputData GetInputs(string folder)
    {
      var files = directoryService.GetAllInputFiles(folder);
      var inputs = files.Select(f => new HomeworkChecker.Input(Path.GetFileNameWithoutExtension(f), directoryService.ReadFileContent(f)));

      return new(inputs);
    }
  }
}
