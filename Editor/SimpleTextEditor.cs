using System;
using System.Collections.Generic;

namespace Editor;

public static class SimpleTextEditor
{
    public class EditorSession
    {
        public EditorState State { get; set; } = new();
        public List<Event> Events { get; set; } = new();
        public EditorAggregate Aggregate { get; set; } = new();
        public EditorEventApplier Applier { get; set; } = new();
    }

    public class EditorProcessor
    {
        public Func<string> Input { get; set; }
        public Action<string> Output { get; set; }
        public EditorSession Session { get; set; }

        public void Initialize()
        {
            // this implementation does not validate that number of actual input commands
            // matches declared number
            _ = int.Parse(Input());
        }

        public bool Next()
        {
            var inputString = Input();
            if (string.IsNullOrWhiteSpace(inputString))
                return false;

            var pair = inputString.Split(' ');
            var operation = Enum.Parse<OperationId>(pair[0]);
            switch (operation)
            {
                case OperationId.Append:
                    ProcessCommand(new AppendString { Text = pair[1] });
                    break;
                case OperationId.Delete:
                    ProcessCommand(new DeleteLastCharacters { CharactersCount = int.Parse(pair[1]) });
                    break;
                case OperationId.Print:
                    ProcessCommand(new PrintCharacter { CharacterPosition = int.Parse(pair[1]) });
                    break;
                case OperationId.Undo:
                    ProcessCommand(new UndoLastEdit());
                    break;
                default:
                    throw new InvalidOperationException($"Operation {operation} is not supported");
            }

            return true;
        }

        public void ProcessCommand(AppendString cmd)
        {
            var evt = Session.Aggregate.Append(Session.State, cmd);
            Session.Events.Add(evt);
            Session.State = Session.Applier.Apply(Session.State, evt);
        }

        public void ProcessCommand(DeleteLastCharacters cmd)
        {
            var evt = Session.Aggregate.Delete(Session.State, cmd);
            Session.Events.Add(evt);
            Session.State = Session.Applier.Apply(Session.State, evt);
        }

        public void ProcessCommand(PrintCharacter cmd)
        {
            Output(Session.State.Text[(cmd.CharacterPosition - 1)..(cmd.CharacterPosition)]);
        }

        public void ProcessCommand(UndoLastEdit cmd)
        {
            if (Session.Events.Count == 0)
                return;

            var lastIndex = Session.Events.Count - 1;
            var lastEvent = Session.Events[lastIndex];
            Session.Events.RemoveAt(lastIndex);
            Session.State = Session.Applier.Undo(Session.State, lastEvent);
        }
    }

    public class EditorEventApplier
    {
        public EditorState Apply(EditorState state, Event evt)
        {
            switch (evt)
            {
                case StringAppended appended:
                    state.Text = string.Concat(state.Text, appended.AppendedText);
                    break;
                case LastCharactersDeleted deleted:
                    state.Text = state.Text[..^deleted.CharactersCount];
                    break;
            }
            return state;
        }

        public EditorState Undo(EditorState state, Event evt)
        {
            switch (evt)
            {
                case StringAppended appended:
                    state.Text = state.Text[..^appended.AppendedText.Length];
                    break;
                case LastCharactersDeleted deleted:
                    state.Text = string.Concat(state.Text, deleted.DeletedText);
                    break;
            }
            return state;
        }
    }

    public class EditorAggregate
    {
        public StringAppended Append(EditorState state, AppendString cmd)
        {
            return new StringAppended { AppendedText = cmd.Text };
        }

        public LastCharactersDeleted Delete(EditorState state, DeleteLastCharacters cmd)
        {
            return new LastCharactersDeleted
            {
                CharactersCount = cmd.CharactersCount,
                DeletedText = state.Text[^cmd.CharactersCount..]
            };
        }
    }

    public class EditorState
    {
        public string Text { get; set; } = string.Empty;
    }

    public enum OperationId
    {
        Append = 1,
        Delete = 2,
        Print = 3,
        Undo = 4
    }

    public abstract class Command;

    public class AppendString : Command
    {
        public string Text { get; set; }
    }

    public class DeleteLastCharacters : Command
    {
        public int CharactersCount { get; set; }
    }

    public class PrintCharacter : Command
    {
        // 1-based
        public int CharacterPosition { get; set; }
    }

    public class UndoLastEdit : Command;

    public abstract class Event;

    public class StringAppended : Event
    {
        public string AppendedText { get; set; }
    }

    public class LastCharactersDeleted : Event
    {
        public int CharactersCount { get; set; }
        public string DeletedText { get; set; }
    }
}
