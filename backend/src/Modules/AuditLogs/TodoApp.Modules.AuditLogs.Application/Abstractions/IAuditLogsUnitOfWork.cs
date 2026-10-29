using TodoApp.Shared.Kernel;

namespace TodoApp.Modules.AuditLogs.Application.Abstractions;

/// <summary>Unit of work scoped to the AuditLogs module's own database context.</summary>
public interface IAuditLogsUnitOfWork : IUnitOfWork;
