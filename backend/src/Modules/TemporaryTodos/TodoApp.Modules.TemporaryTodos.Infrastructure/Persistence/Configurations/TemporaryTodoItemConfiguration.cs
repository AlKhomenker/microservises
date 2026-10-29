using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Modules.TemporaryTodos.Domain;

namespace TodoApp.Modules.TemporaryTodos.Infrastructure.Persistence.Configurations;

internal sealed class TemporaryTodoItemConfiguration : IEntityTypeConfiguration<TemporaryTodoItem>
{
    /* --- Methods --- */
    public void Configure(EntityTypeBuilder<TemporaryTodoItem> builder) // настраивает таблицу temporary_todos_items: ключ, столбцы и индекс
    {
        // Имена таблиц начинаются с имени модуля, потому что в SQLite нет схем.
        builder.ToTable("temporary_todos_items");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Text)
            .HasMaxLength(TemporaryTodoItem.TextMaxLength)
            .IsRequired();

        // DateOnly хранится как текст yyyy-MM-dd, поэтому сортировка по строке совпадает с сортировкой по дате.
        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.HasIndex(x => x.StartDate);
    }
}
