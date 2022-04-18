using FluentAssertions;
using HomeworkCheckerLib;
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

    [Fact]
    public void Can_generate_text_difference_on_equal_files()
    {
      var text = "Text";

      var diff = TextDiffGenerator.GenerateDiff(text, text);

      diff.Diffs.Should().BeEmpty();
    }

    [Fact]
    public void Can_get_string_without_whitespaces()
    {
      var text = " Some Text ";

      var withoutWhitespaces = TextDiffGenerator.GetStringWithoutWhiteSpaces(text);

      withoutWhitespaces.Should().Be("SomeText");
    }
  }
}
