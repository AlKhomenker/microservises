using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoApp.Modules.AuditLogs.Domain;

namespace TodoApp.Modules.AuditLogs.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    /* --- Methods --- */
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder) // настраивает таблицу audit_logs_entries: ключ, столбцы и индекс
    {
        // Имена таблиц начинаются с имени модуля, потому что в SQLite нет схем.
        builder.ToTable("audit_logs_entries");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        // Тип хранится строкой, чтобы данные читались без кода и не ломались при смене порядка в enum.
        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // DateOnly хранится как текст yyyy-MM-dd, поэтому сортировка по строке совпадает с сортировкой по дате.
        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.ListName)
            .HasMaxLength(AuditLogEntry.ListNameMaxLength)
            .IsRequired();

        builder.Property(x => x.ItemId)
            .IsRequired();

        builder.HasIndex(x => x.Date);
    }
}
