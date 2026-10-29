using Microsoft.EntityFrameworkCore;
using TodoApp.Modules.Todos.Application.Abstractions;
using TodoApp.Modules.Todos.Domain;

namespace TodoApp.Modules.Todos.Infrastructure.Persistence;

internal sealed class TodosDbContext(DbContextOptions<TodosDbContext> options) // primary-конструктор с настройками DbContext
    : DbContext(options), ITodosUnitOfWork
{
    /* --- Properties --- */
    internal const string MigrationsHistoryTable = "__EFMigrationsHistory_Todos"; // таблица истории миграций этого модуля
    public DbSet<TodoItem> TodoItems => Set<TodoItem>(); // таблица задач

    /* --- Methods --- */
    protected override void OnModelCreating(ModelBuilder modelBuilder) // применяет все конфигурации сущностей из этой сборки
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodosDbContext).Assembly);
    }
}
