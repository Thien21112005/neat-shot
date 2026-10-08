using NeatShot.Core.Services;
using Xunit;

namespace NeatShot.Tests.Services;

public class UndoStackTests
{
    [Fact]
    public void InitialState_ShouldBeEmpty()
    {
        var stack = new UndoStack<string>();

        Assert.Equal(0, stack.Count);
        Assert.False(stack.CanUndo);
        Assert.Empty(stack.Items);
    }

    [Fact]
    public void Push_ShouldIncreaseCountAndSetCanUndo()
    {
        var stack = new UndoStack<string>();

        stack.Push("item1");
        stack.Push("item2");

        Assert.Equal(2, stack.Count);
        Assert.True(stack.CanUndo);
        Assert.Equal(new[] { "item1", "item2" }, stack.Items);
    }

    [Fact]
    public void Pop_ShouldReturnLastItemInLIFOOrder()
    {
        var stack = new UndoStack<int>();
        stack.Push(10);
        stack.Push(20);
        stack.Push(30);

        var popped1 = stack.Pop();
        Assert.Equal(30, popped1);
        Assert.True(stack.CanUndo);
        Assert.Equal(2, stack.Count);

        var popped2 = stack.Pop();
        Assert.Equal(20, popped2);

        var popped3 = stack.Pop();
        Assert.Equal(10, popped3);
        Assert.False(stack.CanUndo);
        Assert.Equal(0, stack.Count);
    }

    [Fact]
    public void Pop_WhenEmpty_ShouldReturnDefault()
    {
        var stack = new UndoStack<string>();

        var result = stack.Pop();

        Assert.Null(result);
        Assert.False(stack.CanUndo);
    }

    [Fact]
    public void Clear_ShouldResetAllItems()
    {
        var stack = new UndoStack<string>();
        stack.Push("a");
        stack.Push("b");

        stack.Clear();

        Assert.Equal(0, stack.Count);
        Assert.False(stack.CanUndo);
        Assert.Empty(stack.Items);
    }

    [Fact]
    public void StateChanged_ShouldFireOnPushPopAndClear()
    {
        var stack = new UndoStack<int>();
        var eventCount = 0;
        stack.StateChanged += (s, e) => eventCount++;

        stack.Push(1);   // +1
        stack.Push(2);   // +1
        stack.Pop();     // +1
        stack.Clear();   // +1

        Assert.Equal(4, eventCount);
    }

    [Fact]
    public void Redo_ShouldRestorePoppedItem_AndResetOnNewPush()
    {
        var stack = new UndoStack<string>();
        stack.Push("A");
        stack.Push("B");

        Assert.False(stack.CanRedo);

        // Undo B
        var undone = stack.Pop();
        Assert.Equal("B", undone);
        Assert.True(stack.CanRedo);
        Assert.Single(stack.Items);

        // Redo B
        var redone = stack.Redo();
        Assert.Equal("B", redone);
        Assert.False(stack.CanRedo);
        Assert.Equal(2, stack.Count);
        Assert.Equal(new[] { "A", "B" }, stack.Items);

        // Undo again, then push new item -> Redo history should be wiped
        stack.Pop();
        Assert.True(stack.CanRedo);
        stack.Push("C");
        Assert.False(stack.CanRedo);
        Assert.Equal(new[] { "A", "C" }, stack.Items);
    }
}
