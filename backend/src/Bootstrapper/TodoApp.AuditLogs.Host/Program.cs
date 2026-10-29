using TodoApp.Modules.AuditLogs.Api;
using TodoApp.SharedService;

// Читает конфигурацию: appsettings.json -> appsettings.{Environment}.json -> env -> args.
var builder = WebApplication.CreateBuilder(args);

// ===== Модуль =====
// Этот сервис запускает только модуль AuditLogs. Хост знает его только через IModule.
var module = new AuditLogsModule();

// ===== DI =====
// Общие сервисы хоста: TimeProvider, ProblemDetails, события, OpenAPI, health checks, CORS (TodoApp.SharedService).
// События ListItemChanged из Todos сюда не приходят: публикатор работает только внутри процесса.
builder.AddSharedServices();
// Application-слой, DbContext (SQLite), обработчик ListItemChanged, health check audit-logs-db, мигратор БД.
module.RegisterServices(builder.Services, builder.Configuration);

// Строит DI-контейнер. После этого регистрировать сервисы уже нельзя.
var app = builder.Build();

// ===== Middleware и endpoints хоста =====
// Обработка ошибок, OpenAPI, CORS, /health/live и /health/ready (TodoApp.SharedService).
app.UseSharedServices();

// ===== Endpoints модуля =====
// Маршруты журнала (MapAuditLogEndpoints).
module.MapEndpoints(app);

// Запускает hosted services (мигратор применяет миграции, если включено), затем Kestrel.
await app.RunAsync();

// Делает точку входа видимой для WebApplicationFactory в интеграционных тестах.
public partial class Program;
