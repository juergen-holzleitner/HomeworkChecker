using FluentAssertions;
using HomeworkCheckerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HomeworkCheckerLibTest
{
  public class TestTextDifferences
  {
    [Fact]
    public void Can_generate_text_differences()
    {
      var oldText = "Text";
      var newText = "TextX";

      var diff = TextDiffGenerator.GenerateDiff(oldText, newText);

      diff.Diffs.Should().Contain(new DiffMatchPatch.Diff(DiffMatchPatch.Operation.INSERT, "X"));
    }
  }
}
