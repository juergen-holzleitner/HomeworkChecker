using HomeworkCheckerLib;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestGeneralUsage
  {
    [Fact]
    public void Can_create_HomeworkChecker_to_process_Master_folder()
    {
      var sut = new HomeworkChecker();

      sut.ProcessMaster();
    }
  }
}