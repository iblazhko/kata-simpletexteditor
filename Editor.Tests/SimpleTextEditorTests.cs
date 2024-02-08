namespace Editor.Tests;

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

public class TextEditorTests
{
    [Fact]
    public void TextEditorSessionTest()
    {
        var input = new Queue<string>(["8", "1 abc", "3 3", "2 3", "1 xy", "3 2", "4", "4", "3 1"]);
        var expectedOutput = new List<string>(["c", "y", "a"]);

        var output = new List<string>();
        var runner = new EditorSessionStubRunner(input, output);
        runner.Start();

        output.Should().Equal(expectedOutput);
    }
}
