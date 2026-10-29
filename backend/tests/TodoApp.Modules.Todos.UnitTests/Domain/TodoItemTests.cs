using TodoApp.Modules.Todos.Domain;

namespace TodoApp.Modules.Todos.UnitTests.Domain;

public sealed class TodoItemTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidText_ReturnsItem()
    {
        var result = TodoItem.Create("Buy milk", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Buy milk", result.MyValue.Text);
        Assert.Equal(Now, result.MyValue.CreateDate);
        Assert.NotEqual(Guid.Empty, result.MyValue.Id);
    }

    [Fact]
    public void Create_TrimsText()
    {
        var result = TodoItem.Create("  Buy milk \n", Now);

        Assert.Equal("Buy milk", result.MyValue.Text);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var first = TodoItem.Create("a", Now).MyValue;
        var second = TodoItem.Create("b", Now).MyValue;

        Assert.NotEqual(first.Id, second.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutText_ReturnsTextRequired(string? text)
    {
        var result = TodoItem.Create(text, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal(TodoErrors.TextRequired, result.Error);
    }

    [Fact]
    public void Create_WithTextAtMaxLength_Succeeds()
    {
        var result = TodoItem.Create(new string('x', TodoItem.TextMaxLength), Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithTooLongText_ReturnsTextTooLong()
    {
        var result = TodoItem.Create(new string('x', TodoItem.TextMaxLength + 1), Now);

        Assert.False(result.IsSuccess);
        Assert.Equal(TodoErrors.TextTooLong, result.Error);
    }
}
