using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestPercentageAdder
  {
    [Fact]
    public void Can_run_PercentageAdder_on_folder()
    {
      var fileEnumerator = new Mock<FilesystemService.IFileEnumerator>();

      const string folder = "homeworkFolder";
      const string fileName = "homeworkFolder\\fileA.java";

      fileEnumerator.Setup(f => f.GetFilesInFolderRecursivly(folder, It.IsAny<string>())).Returns(new List<string> { fileName });
      fileEnumerator.Setup(f => f.ReadFileContent(fileName)).Returns(@"some code
// Todo: some leftovers
// some issue [-10%]

");
      var output = new Mock<IRuntimeOutput>();

      var sut = new PercentageAdder(fileEnumerator.Object, output.Object);

      sut.ProcessPercentages(folder);

      fileEnumerator.Verify(f => f.GetFilesInFolderRecursivly(folder, "*.java"));
      fileEnumerator.Verify(f => f.ReadFileContent(fileName));

      fileEnumerator.Verify(f => f.WriteAllText(fileName, @"some code
// Todo: some leftovers
// some issue [-10%]
// [Total: 90%]

"));

      output.Verify(o => o.WriteWarning("fileA.java:2: Todo: some leftovers"));
    }

    [Fact]
    public void Can_update_existing_percentage()
    {
      var fileEnumerator = new Mock<FilesystemService.IFileEnumerator>();

      const string folder = "homeworkFolder";
      const string fileName = "homeworkFolder\\fileA.java";

      fileEnumerator.Setup(f => f.GetFilesInFolderRecursivly(folder, It.IsAny<string>())).Returns(new List<string> { fileName });
      fileEnumerator.Setup(f => f.ReadFileContent(fileName)).Returns(@"some code
// Todo: some leftovers
// some issue [-10%]
// [Total: 50%]
");
      var output = new Mock<IRuntimeOutput>();

      var sut = new PercentageAdder(fileEnumerator.Object, output.Object);

      sut.ProcessPercentages(folder);

      fileEnumerator.Verify(f => f.GetFilesInFolderRecursivly(folder, "*.java"));
      fileEnumerator.Verify(f => f.ReadFileContent(fileName));

      fileEnumerator.Verify(f => f.WriteAllText(fileName, @"some code
// Todo: some leftovers
// some issue [-10%]
// [Total: 90%]
"));

      output.Verify(o => o.WriteWarning("fileA.java:2: Todo: some leftovers"));
    }

    [Fact]
    public void Can_add_text_after_line()
    {
      var fileEnumerator = new Mock<FilesystemService.IFileEnumerator>();
      const string fileName = "fileA.java";

      string folder = "homeworkFolder";
      fileEnumerator.Setup(f => f.GetFilesInFolderRecursivly(folder, It.IsAny<string>())).Returns(new List<string> { fileName });
      fileEnumerator.Setup(f => f.ReadFileContent(fileName)).Returns("old text");

      var sut = new PercentageAdder(fileEnumerator.Object, Mock.Of<IRuntimeOutput>());

      sut.AddText(fileName, 1, "new text\n");

      fileEnumerator.Verify(f => f.WriteAllText(fileName, "old text\r\nnew text\n"));
    }

    [Fact]
    public void Can_replace_text_at_line()
    {
      var fileEnumerator = new Mock<FilesystemService.IFileEnumerator>();
      const string fileName = "fileA.java";

      string folder = "homeworkFolder";
      fileEnumerator.Setup(f => f.GetFilesInFolderRecursivly(folder, It.IsAny<string>())).Returns(new List<string> { fileName });
      fileEnumerator.Setup(f => f.ReadFileContent(fileName)).Returns("old text");

      var sut = new PercentageAdder(fileEnumerator.Object, Mock.Of<IRuntimeOutput>());

      sut.ReplaceText(fileName, 1, "new text\n");

      fileEnumerator.Verify(f => f.WriteAllText(fileName, "new text\n"));
    }

    [Theory]
    [InlineData("some text // [-10%]", -10)]
    [InlineData("some text // some comment", null)]
    [InlineData("// [-10%] // [-5%]", -15)]
    public void Can_get_percentage_value_from_line(string line, int? expectedValue)
    {
      var sut = new PercentageAdder(Mock.Of<FilesystemService.IFileEnumerator>(), Mock.Of<IRuntimeOutput>());

      var percentage = sut.GetPercentageFromLine("fileA.java", 1, line);

      percentage.Should().Be(expectedValue);
    }

    [Fact]
    public void Print_warning_if_strange_percentage_value()
    {
      var outputMock = new Mock<IRuntimeOutput>();

      const string line = "// [0%]";
      var sut = new PercentageAdder(Mock.Of<FilesystemService.IFileEnumerator>(), outputMock.Object);

      sut.GetPercentageFromLine("fileA.java", 1, line);

      outputMock.Verify(o => o.WriteWarning("fileA.java:1: unusual percentage 0%"));
    }

    [Fact]
    public void Print_warning_if_line_has_TODO()
    {
      var outputMock = new Mock<IRuntimeOutput>();

      const string line = "// TODO: fix this";
      var sut = new PercentageAdder(Mock.Of<FilesystemService.IFileEnumerator>(), outputMock.Object);

      sut.GeneralLineCheck("fileA.java", 1, line);

      outputMock.Verify(o => o.WriteWarning("fileA.java:1: TODO: fix this"));
    }

    [Theory]
    [InlineData("// [Total: 50%]", 50)]
    [InlineData("// [50%]", null)]
    public void Can_determine_line_with_total_value(string line, int? expectedValue)
    {
      var totalPercentage = PercentageAdder.GetTotalPercentageValue(line);

      totalPercentage.Should().Be(expectedValue);
    }

    [Fact]
    public void Print_warning_if_final_percentage_is_invalid()
    {
      var fileEnumerator = new Mock<FilesystemService.IFileEnumerator>();

      const string folder = "homeworkFolder";
      const string fileName = "homeworkFolder\\fileA.java";

      fileEnumerator.Setup(f => f.GetFilesInFolderRecursivly(folder, It.IsAny<string>())).Returns(new List<string> { fileName });
      fileEnumerator.Setup(f => f.ReadFileContent(fileName)).Returns(@"some code
// Todo: some leftovers
// some issue [-70%] // some other [-40%]
");
      var output = new Mock<IRuntimeOutput>();

      var sut = new PercentageAdder(fileEnumerator.Object, output.Object);

      sut.ProcessPercentages(folder);

      output.Verify(o => o.WriteWarning($"fileA.java: invalid final percentage -10%"));
    }

    [Fact]
    public void Print_final_percentages()
    {
      var fileEnumerator = new Mock<FilesystemService.IFileEnumerator>();

      const string folder = "homeworkFolder";
      const string fileName = "homeworkFolder\\fileA.java";

      fileEnumerator.Setup(f => f.GetFilesInFolderRecursivly(folder, It.IsAny<string>())).Returns(new List<string> { fileName });
      fileEnumerator.Setup(f => f.ReadFileContent(fileName)).Returns(@"some code
// Todo: some leftovers
// some issue [-70%]
");
      var output = new Mock<IRuntimeOutput>();

      var sut = new PercentageAdder(fileEnumerator.Object, output.Object);

      sut.ProcessPercentages(folder);

      output.Verify(o => o.WriteSuccess("fileA.java: 30%"));
    }

    [Fact]
    public void Print_updates_of_final_percentages()
    {
      var fileEnumerator = new Mock<FilesystemService.IFileEnumerator>();

      const string folder = "homeworkFolder";
      const string fileName = "homeworkFolder\\fileA.java";

      fileEnumerator.Setup(f => f.GetFilesInFolderRecursivly(folder, It.IsAny<string>())).Returns(new List<string> { fileName });
      fileEnumerator.Setup(f => f.ReadFileContent(fileName)).Returns(@"some code
// Todo: some leftovers
// some issue [-70%]
// [Total: 50%]
");
      var output = new Mock<IRuntimeOutput>();

      var sut = new PercentageAdder(fileEnumerator.Object, output.Object);

      sut.ProcessPercentages(folder);

      output.Verify(o => o.WriteSuccess("fileA.java: (updated from 50%): 30%"));
    }

    [Fact]
    public void Print_updates_of_final_percentages_if_other_lines_follow()
    {
      var fileEnumerator = new Mock<FilesystemService.IFileEnumerator>();

      const string folder = "homeworkFolder";
      const string fileName = "homeworkFolder\\fileA.java";

      fileEnumerator.Setup(f => f.GetFilesInFolderRecursivly(folder, It.IsAny<string>())).Returns(new List<string> { fileName });
      fileEnumerator.Setup(f => f.ReadFileContent(fileName)).Returns(@"some code
// Todo: some leftovers
// some issue [-70%]
// [Total: 50%]
// some other text
");
      var output = new Mock<IRuntimeOutput>();

      var sut = new PercentageAdder(fileEnumerator.Object, output.Object);

      sut.ProcessPercentages(folder);

      output.Verify(o => o.WriteSuccess("fileA.java: (updated from 50%): 30%"));
    }

    [Fact]
    public void Do_not_update_if_final_value_is_already_set()
    {
      var fileEnumerator = new Mock<FilesystemService.IFileEnumerator>();

      const string folder = "homeworkFolder";
      const string fileName = "homeworkFolder\\fileA.java";

      fileEnumerator.Setup(f => f.GetFilesInFolderRecursivly(folder, It.IsAny<string>())).Returns(new List<string> { fileName });
      fileEnumerator.Setup(f => f.ReadFileContent(fileName)).Returns(@"some code
// Todo: some leftovers
// some issue [-70%]
// [Total: 30%]
");
      var output = new Mock<IRuntimeOutput>();

      var sut = new PercentageAdder(fileEnumerator.Object, output.Object);

      sut.ProcessPercentages(folder);

      output.Verify(o => o.WriteSuccess("fileA.java: (already set): 30%"));
      fileEnumerator.Verify(f => f.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
  }
}
