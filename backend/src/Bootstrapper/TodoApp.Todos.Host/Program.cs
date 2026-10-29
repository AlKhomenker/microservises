using TodoApp.Modules.Todos.Api;
using TodoApp.SharedService;

// Читает конфигурацию: appsettings.json -> appsettings.{Environment}.json -> env -> args.
var builder = WebApplication.CreateBuilder(args);

// ===== Модуль =====
// Этот сервис запускает только модуль Todos. Хост знает его только через IModule.
var module = new TodosModule();

// ===== DI =====
// Общие сервисы хоста: TimeProvider, ProblemDetails, события, OpenAPI, health checks, CORS (TodoApp.SharedService).
// Получателя ListItemChanged (AuditLogs) в этом процессе нет, поэтому события публикуются без обработки.
builder.AddSharedServices();
// Application-слой, DbContext (SQLite), health check todos-db, мигратор БД.
module.RegisterServices(builder.Services, builder.Configuration);

// Строит DI-контейнер. После этого регистрировать сервисы уже нельзя.
var app = builder.Build();

// ===== Middleware и endpoints хоста =====
// Обработка ошибок, OpenAPI, CORS, /health/live и /health/ready (TodoApp.SharedService).
app.UseSharedServices();

// ===== Endpoints модуля =====
// Маршруты задач (MapTodoEndpoints).
module.MapEndpoints(app);

// Запускает hosted services (мигратор применяет миграции, если включено), затем Kestrel.
await app.RunAsync();

// Делает точку входа видимой для WebApplicationFactory в интеграционных тестах.
public partial class Program;
