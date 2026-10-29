using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Modules.Todos.Domain;

namespace TodoApp.Modules.Todos.Infrastructure.Persistence.Configurations;

internal sealed class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    /* --- Methods --- */
    public void Configure(EntityTypeBuilder<TodoItem> builder) // настраивает таблицу todos_items: ключ, столбцы и индекс
    {
        // Имена таблиц начинаются с имени модуля, потому что в SQLite нет схем.
        builder.ToTable("todos_items");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Text)
            .HasMaxLength(TodoItem.TextMaxLength)
            .IsRequired();

        // SQLite не умеет сортировать DateTimeOffset, сохранённые как текст,
        // поэтому храним их как UTC ticks: точно и с сортировкой.
        builder.Property(x => x.CreateDate)
            .HasConversion(v => v.UtcTicks, v => new DateTimeOffset(v, TimeSpan.Zero))
            .IsRequired();

        builder.HasIndex(x => x.CreateDate);
    }
}
