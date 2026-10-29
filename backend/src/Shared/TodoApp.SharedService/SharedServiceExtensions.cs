using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TodoApp.Shared.Abstractions.Events;

namespace TodoApp.SharedService;

/// <summary>
/// Setup that every service host shares: ProblemDetails, CORS, health checks and OpenAPI.
/// A host calls AddSharedServices before it registers its module, and UseSharedServices before it maps the module's endpoints.
/// </summary>
public static class SharedServiceExtensions
{
    /* --- Properties --- */
    private const string FrontendCorsPolicy = "Frontend"; // имя CORS-политики для фронтенда

    /* --- Methods --- */
    public static WebApplicationBuilder AddSharedServices(this WebApplicationBuilder builder) // регистрирует в DI общие сервисы хоста; возвращает builder для цепочки вызовов
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Единый источник времени; в тестах подменяется на FakeTimeProvider.
        builder.Services.AddSingleton(TimeProvider.System);
        // Ошибки отдаются в формате ProblemDetails (RFC 7807).
        builder.Services.AddProblemDetails();
        // События (ListItemChanged и др.) обрабатываются только внутри этого процесса.
        // Получатели из модулей в других сервисах их не увидят.
        builder.Services.AddInProcessEvents();
        // Генерация OpenAPI-документа по endpoints.
        builder.Services.AddOpenApi();
        // Инфраструктура health checks; сами проверки добавляют модули (например, todos-db).
        builder.Services.AddHealthChecks();
        // CORS для фронтенда; разрешённые origins берутся из Cors:AllowedOrigins.
        builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod()));

        return builder;
    }

    public static WebApplication UseSharedServices(this WebApplication app) // подключает общий middleware (порядок важен) и endpoints хоста; возвращает app для цепочки вызовов
    {
        ArgumentNullException.ThrowIfNull(app);

        // Необработанное исключение -> 500 в формате ProblemDetails. Стоит первым, чтобы ловить ошибки всего конвейера.
        app.UseExceptionHandler();
        // Пустые ответы 4xx/5xx (например, 404) тоже получают тело ProblemDetails.
        app.UseStatusCodePages();

        // OpenAPI-документ (/openapi/v1.json) открыт только в Development.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // Добавляет CORS-заголовки для запросов с фронтенда.
        app.UseCors(FrontendCorsPolicy);

        // Liveness: проверки не запускаются, ответ значит только "процесс жив".
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        // Readiness: запускаются проверки с тегом "ready" (доступность БД).
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

        return app;
    }
}
