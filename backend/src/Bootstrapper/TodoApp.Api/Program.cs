using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using TodoApp.Modules.AuditLogs.Api;
using TodoApp.Modules.TemporaryTodos.Api;
using TodoApp.Modules.Todos.Api;
using TodoApp.Shared.Abstractions.Events;
using TodoApp.Shared.Abstractions.Modules;

const string FrontendCorsPolicy = "Frontend";

// Читает конфигурацию: appsettings.json -> appsettings.{Environment}.json -> env -> args.
var builder = WebApplication.CreateBuilder(args);

// ===== Модули =====
// Каждый модуль регистрируется здесь и больше нигде.
// Хост знает модуль только через IModule: RegisterServices (DI) и MapEndpoints (маршруты).
IModule[] modules = [new TodosModule(), new TemporaryTodosModule(), new AuditLogsModule()];

// ===== DI: общие сервисы хоста =====
// Единый источник времени; в тестах подменяется на FakeTimeProvider.
builder.Services.AddSingleton(TimeProvider.System);
// Ошибки отдаются в формате ProblemDetails (RFC 7807).
builder.Services.AddProblemDetails();
// События между модулями (ListItemChanged и др.): модуль публикует, другие модули обрабатывают, ссылок друг на друга нет.
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

// ===== DI: сервисы модулей =====
// TodosModule: Application-слой, DbContext (SQLite), health check todos-db, мигратор БД.
// TemporaryTodosModule: то же самое, но со своей БД (temporary-todos.db) и health check temporary-todos-db.
// AuditLogsModule: своя БД (audit-logs.db) и обработчик ListItemChanged, который пишет запись в журнал.
foreach (var module in modules)
{
    module.RegisterServices(builder.Services, builder.Configuration);
}

// Строит DI-контейнер. После этого регистрировать сервисы уже нельзя.
var app = builder.Build();

// ===== Middleware (порядок важен) =====
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

// ===== Endpoints хоста =====
// Liveness: проверки не запускаются, ответ значит только "процесс жив".
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
// Readiness: запускаются проверки с тегом "ready" (доступность БД).
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

// ===== Endpoints модулей =====
// TodosModule добавляет маршруты задач (MapTodoEndpoints).
// TemporaryTodosModule добавляет маршруты временных задач (MapTemporaryTodoEndpoints).
// AuditLogsModule добавляет маршруты журнала (MapAuditLogEndpoints).
foreach (var module in modules)
{
    module.MapEndpoints(app);
}

// Запускает hosted services (мигратор каждого модуля применяет миграции, если включено), затем Kestrel.
await app.RunAsync();

// Делает точку входа видимой для WebApplicationFactory в интеграционных тестах.
public partial class Program;
