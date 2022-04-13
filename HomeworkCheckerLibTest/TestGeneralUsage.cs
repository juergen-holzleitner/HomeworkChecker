using HomeworkCheckerLib;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestGeneralUsage
  {
    [Fact]
    public void Can_create_HomeworkChecker_to_process_Master_folder()
    {
      const string masterFolder = @"arbitraryFolder";
      var sut = new HomeworkChecker();

      sut.ProcessMaster(masterFolder);
    }
  }
}