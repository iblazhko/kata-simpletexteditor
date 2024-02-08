using System;
using System.Collections.Generic;

namespace Editor;

public abstract class EditorSessionRunner
{
    protected SimpleTextEditor.EditorProcessor Processor { get; }

    protected EditorSessionRunner(SimpleTextEditor.EditorProcessor processor)
    {
        Processor = processor;
    }

    public void Start()
    {
        Processor.Initialize();
        while (Processor.Next()) { }
    }
}

public class EditorSessionConsoleRunner : EditorSessionRunner
{
    public EditorSessionConsoleRunner()
        : base(
            new SimpleTextEditor.EditorProcessor
            {
                Input = () => Console.ReadLine() ?? string.Empty,
                Output = Console.WriteLine,
                Session = new SimpleTextEditor.EditorSession()
            }
        ) { }
}

public class EditorSessionStubRunner : EditorSessionRunner
{
    public Queue<string> Input { get; }
    public List<string> Output { get; }

    public EditorSessionStubRunner(Queue<string> input, List<string> output)
        : base(
            new SimpleTextEditor.EditorProcessor
            {
                Input = () => input.TryDequeue(out var s) ? s : string.Empty,
                Output = s => output.Add(s),
                Session = new SimpleTextEditor.EditorSession()
            }
        )
    {
        Input = input;
        Output = output;
    }
}
