using System.Reflection;
using NetArchTest.Rules;
using TodoApp.Modules.AuditLogs.Api;
using TodoApp.Modules.AuditLogs.Application.GetAuditLogs;
using TodoApp.Modules.AuditLogs.Domain;
using TodoApp.Modules.TemporaryTodos.Api;
using TodoApp.Modules.TemporaryTodos.Application.GetTemporaryTodos;
using TodoApp.Modules.TemporaryTodos.Domain;
using TodoApp.Modules.Todos.Api;
using TodoApp.Modules.Todos.Application.GetTodos;
using TodoApp.Modules.Todos.Domain;
using AuditLogsInfrastructure = TodoApp.Modules.AuditLogs.Infrastructure.DependencyInjection;
using TemporaryTodosInfrastructure = TodoApp.Modules.TemporaryTodos.Infrastructure.DependencyInjection;
using TodosInfrastructure = TodoApp.Modules.Todos.Infrastructure.DependencyInjection;

namespace TodoApp.ArchitectureTests;

/// <summary>
/// Modules never reference each other. They talk only through the host and
/// through shared integration events in TodoApp.Shared.Kernel.
/// </summary>
public sealed class ModuleIndependenceTests
{
    private static readonly Dictionary<string, Assembly[]> Modules = new()
    {
        ["Todos"] =
        [
            typeof(TodoItem).Assembly,
            typeof(GetTodosHandler).Assembly,
            typeof(TodosInfrastructure).Assembly,
            typeof(TodosModule).Assembly,
        ],
        ["TemporaryTodos"] =
        [
            typeof(TemporaryTodoItem).Assembly,
            typeof(GetTemporaryTodosHandler).Assembly,
            typeof(TemporaryTodosInfrastructure).Assembly,
            typeof(TemporaryTodosModule).Assembly,
        ],
        ["AuditLogs"] =
        [
            typeof(AuditLogEntry).Assembly,
            typeof(GetAuditLogsHandler).Assembly,
            typeof(AuditLogsInfrastructure).Assembly,
            typeof(AuditLogsModule).Assembly,
        ],
    };

    public static TheoryData<string, string> ModulePairs()
    {
        var pairs = new TheoryData<string, string>();
        foreach (var module in Modules.Keys)
        {
            foreach (var other in Modules.Keys.Where(other => other != module))
            {
                pairs.Add(module, other);
            }
        }

        return pairs;
    }

    [Theory]
    [MemberData(nameof(ModulePairs))]
    public void Module_DoesNotDependOnOtherModule(string module, string other)
    {
        var result = Types.InAssemblies(Modules[module])
            .ShouldNot()
            .HaveDependencyOnAny(LayerNamespaces($"TodoApp.Modules.{other}"))
            .GetResult();

        AssertSuccessful(result);
    }

    private static string[] LayerNamespaces(string prefix) =>
        [$"{prefix}.Domain", $"{prefix}.Application", $"{prefix}.Infrastructure", $"{prefix}.Api"];

    private static void AssertSuccessful(NetArchTest.Rules.TestResult result) =>
        Assert.True(
            result.IsSuccessful,
            "Violating types: " + string.Join(", ", result.FailingTypeNames ?? []));
}
