using TodoApp.Modules.TemporaryTodos.Domain;

namespace TodoApp.Modules.TemporaryTodos.UnitTests.Domain;

public sealed class TemporaryTodoItemTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 10, 21);

    [Fact]
    public void Create_WithValidInput_ReturnsItem()
    {
        var result = TemporaryTodoItem.Create("Buy milk", Today, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Buy milk", result.MyValue.Text);
        Assert.Equal(Today, result.MyValue.StartDate);
        Assert.NotEqual(Guid.Empty, result.MyValue.Id);
    }

    [Fact]
    public void Create_TrimsText()
    {
        var result = TemporaryTodoItem.Create("  Buy milk \n", Today, Now);

        Assert.Equal("Buy milk", result.MyValue.Text);
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var first = TemporaryTodoItem.Create("a", Today, Now).MyValue;
        var second = TemporaryTodoItem.Create("b", Today, Now).MyValue;

        Assert.NotEqual(first.Id, second.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutText_ReturnsTextRequired(string? text)
    {
        var result = TemporaryTodoItem.Create(text, Today, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal(TemporaryTodoErrors.TextRequired, result.Error);
    }

    [Fact]
    public void Create_WithTextAtMaxLength_Succeeds()
    {
        var result = TemporaryTodoItem.Create(new string('x', TemporaryTodoItem.TextMaxLength), Today, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithTooLongText_ReturnsTextTooLong()
    {
        var result = TemporaryTodoItem.Create(new string('x', TemporaryTodoItem.TextMaxLength + 1), Today, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal(TemporaryTodoErrors.TextTooLong, result.Error);
    }

    [Fact]
    public void Create_WithoutStartDate_ReturnsStartDateRequired()
    {
        var result = TemporaryTodoItem.Create("Buy milk", null, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal(TemporaryTodoErrors.StartDateRequired, result.Error);
    }

    [Theory]
    [InlineData(-1, TemporaryTodoStatus.Expired)]
    [InlineData(0, TemporaryTodoStatus.Active)]
    [InlineData(1, TemporaryTodoStatus.Active)]
    public void GetStatus_DependsOnStartDate(int daysFromToday, TemporaryTodoStatus expected)
    {
        var item = TemporaryTodoItem.Create("a", Today.AddDays(daysFromToday), Now).MyValue;

        Assert.Equal(expected, item.GetStatus(Today));
    }
}
