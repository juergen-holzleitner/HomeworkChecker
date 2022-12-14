using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.IO;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestCheckstyle
  {
    [Fact]
    public void Can_process_checkstyle()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.GetCurrentFolder()).Returns("currentFolder");
      appExecuterMock.Setup(x => x.Execute("java", Path.Combine("currentFolder", "checkstyle"), "-jar \"checkstyle-10.3.3-all.jar\" -c google_checks_modified.xml \"somePath\\file.java\""))
        .Returns(new IAppExecuter.ExecutionResult(0, "Starting audit...\r\nsomePath\\file.java:25: checkstyle outputAudit done.\r\n", false));

      var sut = new CheckstyleProcessor(appExecuterMock.Object);

      var result = sut.Process(@"somePath\file.java");

      result.CheckstyleOutput.Should().Be("file.java:25: checkstyle output");
      result.ExitCode.Should().Be(0);
    }

    [Theory]
    [InlineData("Starting audit...\r\nSomeOutput", "SomeOutput")]
    [InlineData("Audit done.\r\n ", "")]
    public void Can_clean_checkstyle_output(string output, string expectedCleanedOutput)
    {
      var cleanedOutput = CheckstyleProcessor.CleanOutput(output);

      cleanedOutput.Should().Be(expectedCleanedOutput);
    }

    [Fact]
    public void Can_remove_path_from_output()
    {
      var output = @"somePath\someFile.java";

      var cleanedOutput = CheckstyleProcessor.RemovePathsFromOutput(output, @"somePath\");

      cleanedOutput.Should().Be("someFile.java");
    }
  }
}
