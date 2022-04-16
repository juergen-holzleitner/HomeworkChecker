﻿using FluentAssertions;
using HomeworkCheckerLib;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestOutputGeneration
  {
    [Fact]
    public void Can_generate_output()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(a => a.Execute("java", "workingDir", "fileName", "inputContent", 5000))
                     .Returns(new IAppExecuter.ExecutionResult(0, "output content", false));
      var sut = new OutputGenerator(appExecuterMock.Object);

      var output = sut.GenerateOutput("fileName.java", "workingDir", "inputContent");

      output.Should().Be(new OutputGenerator.Output("output content", false));
    }

    [Fact]
    public void Can_handle_timeout()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(a => a.Execute("java", "workingDir", "fileName", "inputContent", 5000))
                     .Returns(new IAppExecuter.ExecutionResult(0, "output content", true));
      var sut = new OutputGenerator(appExecuterMock.Object);

      var output = sut.GenerateOutput("fileName.java", "workingDir", "inputContent");

      output.Should().Be(new OutputGenerator.Output("output content", true));
    }

    [Fact]
    public void Can_generate_output_without_input_files_XXX()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(a => a.Execute("java", "workingDir", "fileName", null, 5000))
                     .Returns(new IAppExecuter.ExecutionResult(0, "output content", false));
      var sut = new OutputGenerator(appExecuterMock.Object);

      var output = sut.GenerateOutput("fileName.java", "workingDir");

      output.Should().Be(new OutputGenerator.Output("output content", false));
    }

    [Fact]
    public void Can_generate_output_without_input_files()
    {
      var appExecuterMock = new Mock<IAppExecuter>();
      appExecuterMock.Setup(x => x.Execute("java", "masterfolder", "program", null, It.IsAny<int>()))
                     .Returns(new IAppExecuter.ExecutionResult(0, "output", false));
      var sut = new HomeworkChecker(Mock.Of<FilesystemService.IFileEnumerator>(), appExecuterMock.Object, Mock.Of<IRuntimeOutput>());
      InputGenerator.InputData emptyInputData = new(new List<HomeworkChecker.Input>());

      var outputs = sut.GetProgramOutputs("program.java", "masterfolder", emptyInputData);

      outputs.Should().Equal(new List<HomeworkChecker.Output> { new(new("<no input>", string.Empty), "output", false) });

    }

  }
}
