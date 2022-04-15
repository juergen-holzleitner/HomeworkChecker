namespace HomeworkCheckerLib
{
  internal class InputGenerator
  {
    private readonly FilesystemService directoryService;

    internal record Input(string Filename);
    internal record InputData(IEnumerable<Input> Inputs);

    public InputGenerator(FilesystemService directoryService)
    {
      this.directoryService = directoryService;
    }

    internal InputData GetInputs(string folder)
    {
      var files = directoryService.GetAllInputFiles(folder);
      var inputs = from f in files select new Input(Path.GetFileNameWithoutExtension(f));

      return new(inputs);
    }
  }
}
