using Microsoft.EntityFrameworkCore;
using TodoApp.Modules.TemporaryTodos.Application.Abstractions;
using TodoApp.Modules.TemporaryTodos.Domain;

namespace TodoApp.Modules.TemporaryTodos.Infrastructure.Persistence;

internal sealed class TemporaryTodosDbContext(DbContextOptions<TemporaryTodosDbContext> options) // primary-конструктор с настройками DbContext
    : DbContext(options), ITemporaryTodosUnitOfWork
{
    /* --- Properties --- */
    internal const string MigrationsHistoryTable = "__EFMigrationsHistory_TemporaryTodos"; // таблица истории миграций этого модуля
    public DbSet<TemporaryTodoItem> TemporaryTodoItems => Set<TemporaryTodoItem>(); // таблица временных задач

    /* --- Methods --- */
    protected override void OnModelCreating(ModelBuilder modelBuilder) // применяет все конфигурации сущностей из этой сборки
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TemporaryTodosDbContext).Assembly);
    }
}
