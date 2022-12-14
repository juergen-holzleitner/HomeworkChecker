namespace HomeworkCheckerLib
{
  internal interface IRuntimeOutput
  {
    void WriteSuccess(string message);
    void WriteError(string message);
    void WriteInfo(string message);
    void WriteWarning(string message);
  }
}
