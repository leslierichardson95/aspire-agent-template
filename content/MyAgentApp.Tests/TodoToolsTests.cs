using MyAgentApp.Agent;
using Xunit;

namespace MyAgentApp.Tests;

public class TodoToolsTests
{
    private readonly TodoService _service = new();
    private readonly TodoTools _tools;

    public TodoToolsTests()
    {
        _tools = new TodoTools(_service);
    }

    [Fact]
    public void AddTodo_CreatesItemAndReturnsConfirmation()
    {
        var result = _tools.AddTodo("Buy groceries");

        Assert.Contains("Buy groceries", result);
        Assert.Single(_service.List());
    }

    [Fact]
    public void ListTodos_EmptyList_ReturnsNotFoundMessage()
    {
        var result = _tools.ListTodos();

        Assert.Equal("No todo items found.", result);
    }

    [Fact]
    public void ListTodos_WithItems_ReturnsAllItems()
    {
        _service.Add("Item 1");
        _service.Add("Item 2");

        var result = _tools.ListTodos();

        Assert.Contains("Item 1", result);
        Assert.Contains("Item 2", result);
    }

    [Fact]
    public void CompleteTodo_MarksItemComplete()
    {
        _service.Add("Finish report");

        var result = _tools.CompleteTodo(1);

        Assert.Contains("✓", result);
        Assert.True(_service.GetById(1)!.IsComplete);
    }

    [Fact]
    public void CompleteTodo_InvalidId_ReturnsNotFound()
    {
        var result = _tools.CompleteTodo(999);

        Assert.Contains("not found", result);
    }

    [Fact]
    public void DeleteTodo_RemovesItem()
    {
        _service.Add("Temp item");

        var result = _tools.DeleteTodo(1);

        Assert.Contains("Deleted", result);
        Assert.Empty(_service.List());
    }

    [Fact]
    public void ListTodos_ExcludeCompleted_FiltersCorrectly()
    {
        _service.Add("Done task");
        _service.Add("Pending task");
        _service.Complete(1);

        var result = _tools.ListTodos(includeCompleted: false);

        Assert.DoesNotContain("Done task", result);
        Assert.Contains("Pending task", result);
    }

    [Fact]
    public void AsAIFunctions_ReturnsFourTools()
    {
        var tools = _tools.AsAIFunctions();

        Assert.Equal(4, tools.Count);
    }

    [Fact]
    public void AsAIFunctions_ToolsHaveDescriptions()
    {
        var tools = _tools.AsAIFunctions();

        foreach (var tool in tools)
        {
            Assert.False(string.IsNullOrWhiteSpace(tool.Name), "Tool should have a name");
        }
    }
}
