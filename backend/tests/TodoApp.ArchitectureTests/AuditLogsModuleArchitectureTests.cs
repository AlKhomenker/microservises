using System.Reflection;
using NetArchTest.Rules;
using TodoApp.Modules.AuditLogs.Api;
using TodoApp.Modules.AuditLogs.Application.GetAuditLogs;
using TodoApp.Modules.AuditLogs.Domain;
using InfrastructureDependencyInjection = TodoApp.Modules.AuditLogs.Infrastructure.DependencyInjection;

namespace TodoApp.ArchitectureTests;

public sealed class AuditLogsModuleArchitectureTests
{
    private const string Prefix = "TodoApp.Modules.AuditLogs";

    private static readonly Assembly Domain = typeof(AuditLogEntry).Assembly;
    private static readonly Assembly Application = typeof(GetAuditLogsHandler).Assembly;
    private static readonly Assembly Infrastructure = typeof(InfrastructureDependencyInjection).Assembly;
    private static readonly Assembly ModuleApi = typeof(AuditLogsModule).Assembly;

    [Fact]
    public void Domain_DependsOnNothingButSharedKernel()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                $"{Prefix}.Application",
                $"{Prefix}.Infrastructure",
                $"{Prefix}.Api",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrWeb()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny(
                $"{Prefix}.Infrastructure",
                $"{Prefix}.Api",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnModuleApi()
    {
        var result = Types.InAssembly(Infrastructure)
            .ShouldNot()
            .HaveDependencyOn($"{Prefix}.Api")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Infrastructure_ExposesOnlyItsRegistrationEntryPoint()
    {
        // EF Core generates migrations as public types, so they are ignored here.
        var publicTypes = Infrastructure.GetExportedTypes()
            .Where(t => t.Namespace != $"{Prefix}.Infrastructure.Persistence.Migrations");

        Assert.Equal([typeof(InfrastructureDependencyInjection)], publicTypes);
    }

    [Fact]
    public void ModuleEndpoints_AreNotPublic()
    {
        var result = Types.InAssembly(ModuleApi)
            .That()
            .ResideInNamespace($"{Prefix}.Api.Endpoints")
            .And()
            .AreClasses()
            .And()
            .AreStatic()
            .Should()
            .NotBePublic()
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Handlers_AreSealed()
    {
        var result = Types.InAssembly(Application)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .BeSealed()
            .GetResult();

        AssertSuccessful(result);
    }

    private static void AssertSuccessful(NetArchTest.Rules.TestResult result) =>
        Assert.True(
            result.IsSuccessful,
            "Violating types: " + string.Join(", ", result.FailingTypeNames ?? []));
}
