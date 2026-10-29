# TodoApp

A small todo list: a .NET 10 backend built as a **modular monolith** and a one-page **React** frontend.

## Run

Needs the .NET 10 SDK and Node.js. Start the backend first, then the frontend, each in its own terminal.

### Backend

http://localhost:5000

```bash
cd backend
dotnet run --project src/Bootstrapper/TodoApp.Api
```

OpenAPI document (Development only): http://localhost:5000/openapi/v1.json

### Frontend

http://localhost:5173 (proxies `/api` to the backend)

```bash
cd frontend
npm install
npm run dev
```

### Database

The database is SQLite. There is no server to start. Each module has its own database file in
`backend/src/Bootstrapper/TodoApp.Api/`: `todos.db` (Todos), `temporary-todos.db` (TemporaryTodos)
and `audit-logs.db` (AuditLogs).
In Development the backend creates these files and applies migrations on start, so `dotnet run` is enough.

To manage the database by hand (run from `backend/`):

```bash
dotnet tool restore     # once, installs dotnet-ef

# Apply all migrations (creates todos.db if it does not exist)
dotnet ef database update \
  -p src/Modules/Todos/TodoApp.Modules.Todos.Infrastructure \
  -s src/Bootstrapper/TodoApp.Api

# Delete the database (all data is lost)
dotnet ef database drop \
  -p src/Modules/Todos/TodoApp.Modules.Todos.Infrastructure \
  -s src/Bootstrapper/TodoApp.Api
```

For the other modules use `-p src/Modules/TemporaryTodos/TodoApp.Modules.TemporaryTodos.Infrastructure`
or `-p src/Modules/AuditLogs/TodoApp.Modules.AuditLogs.Infrastructure` instead.

### Debug the backend

Use VS Code with the **C#** and **C# Dev Kit** extensions.

- **Start with the debugger:** open `backend/src/Bootstrapper/TodoApp.Api/Program.cs` and press **F5**.
  If VS Code asks, pick **C#**, then the **TodoApp.Api** project and the **http** launch profile.
- **Attach to a running backend:** start it with `dotnet run` as shown above, then run
  **Debug: Attach to .NET 5+ or .NET Core process** from the Command Palette (`Cmd+Shift+P`)
  and pick **TodoApp.Api**.

Click left of a line number to set a breakpoint, then call the API (for example from the frontend).
Keys: `F10` step over, `F11` step into, `F5` continue, `Shift+F5` stop.

VS Code must find `dotnet` on its `PATH`. If the SDK is in `~/.dotnet`, add it to `~/.zshrc`
and restart VS Code completely:

```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$PATH:$HOME/.dotnet:$HOME/.dotnet/tools"
```

## Backend projects

All backend projects are listed in `backend/TodoApp.slnx`. When you add or remove a project,
update this file with `dotnet sln` (run from `backend/`):

```bash
# Add a new project to the solution, inside a solution folder
dotnet new classlib -o src/Modules/Notes/TodoApp.Modules.Notes.Domain
dotnet sln TodoApp.slnx add src/Modules/Notes/TodoApp.Modules.Notes.Domain \
  --solution-folder src/Modules/Notes

# Add a reference from one project to another
dotnet add src/Modules/Notes/TodoApp.Modules.Notes.Application \
  reference src/Modules/Notes/TodoApp.Modules.Notes.Domain

# Remove a project from the solution (the folder on disk stays, delete it yourself)
dotnet sln TodoApp.slnx remove src/Modules/Notes/TodoApp.Modules.Notes.Domain

# Show all projects in the solution
dotnet sln TodoApp.slnx list
```

## API

| Method | Path              | Body               | Result                                  |
|--------|-------------------|--------------------|-----------------------------------------|
| GET    | `/api/todos`      | –                  | `200` list, newest first                |
| POST   | `/api/todos`      | `{ "text": "..." }`| `201` created item, `400` validation    |
| DELETE | `/api/todos/{id}` | –                  | `204`, `404` if the item does not exist |
| DELETE | `/api/todos`      | –                  | `204`, removes all items                |

Item: `{ "id": "guid", "text": "string (1-500)", "createDate": "ISO 8601" }`.

Temporary todos (separate module, same rules):

| Method | Path                        | Body                                         | Result                                  |
|--------|-----------------------------|----------------------------------------------|-----------------------------------------|
| GET    | `/api/temporary-todos`      | –                                            | `200` list, latest start date first     |
| POST   | `/api/temporary-todos`      | `{ "text": "...", "startDate": "yyyy-MM-dd" }` | `201` created item, `400` validation    |
| DELETE | `/api/temporary-todos/{id}` | –                                            | `204`, `404` if the item does not exist |
| DELETE | `/api/temporary-todos`      | –                                            | `204`, removes all items                |

Item: `{ "id": "guid", "text": "string (1-500)", "startDate": "yyyy-MM-dd", "status": "active" | "expired" }`.
The status is not stored. It is computed on each read: `expired` when `startDate` is before today (UTC), otherwise `active`.

Audit log (separate module). Every add or remove in any list writes one entry; "Remove all" writes one entry per item.

| Method | Path              | Body | Result                                  |
|--------|-------------------|------|-----------------------------------------|
| GET    | `/api/audit-logs` | –    | `200` list, newest first                |
| DELETE | `/api/audit-logs` | –    | `204`, removes all entries              |

Entry: `{ "id": "guid", "type": "add" | "removed", "date": "yyyy-MM-dd" (UTC), "listName": "todos" | "temporary-todos", "itemId": "guid" }`.
Errors use RFC 9457 problem details. Health checks: `/health/live`, `/health/ready`.

## Backend structure

```
backend/
  src/
    Bootstrapper/TodoApp.Api            host: Program.cs, config, loads modules
    Shared/
      TodoApp.Shared.Kernel             Result, Error, IUnitOfWork, integration events (no framework deps)
      TodoApp.Shared.Abstractions       IModule contract, Result -> HTTP mapping, in-process event publisher
    Modules/Todos/
      TodoApp.Modules.Todos.Domain          TodoItem entity and its rules
      TodoApp.Modules.Todos.Application     use cases (Get / Create / Delete handlers), ports
      TodoApp.Modules.Todos.Infrastructure  EF Core + SQLite, repository, migrations (internal)
      TodoApp.Modules.Todos.Api             TodosModule (IModule) and minimal API endpoints
    Modules/TemporaryTodos/             same four layers for temporary todos (own DbContext,
                                        database file and migrations; no references to Todos)
    Modules/AuditLogs/                  same four layers; listens to ListItemChanged and writes the log
  tests/
    TodoApp.Modules.Todos.UnitTests     domain and handler tests (in-memory fakes, FakeTimeProvider)
    TodoApp.Modules.TemporaryTodos.UnitTests  same for the TemporaryTodos module
    TodoApp.Modules.AuditLogs.UnitTests       same for the AuditLogs module
    TodoApp.Api.IntegrationTests        real HTTP pipeline via WebApplicationFactory + in-memory SQLite
    TodoApp.ArchitectureTests           layer and module boundary rules, modules do not reference each other (NetArchTest)
```

Main ideas:

- The host knows a module only through `IModule`. To add a module, create the same four projects
  and add it to the `modules` array in `Program.cs`.
- Each module owns its `DbContext`, tables (prefixed, e.g. `todos_items`) and its own
  migrations history table, so modules can later move to separate databases.
- Modules never reference each other. A module that needs to tell others about a change publishes an
  integration event from `TodoApp.Shared.Kernel.Events` (for example `ListItemChanged`) through `IEventPublisher`
  after its changes are saved. Other modules handle it with `IIntegrationEventConsumer<T>`.
  Events run in the same process and request. A failing consumer is logged and does not fail the request.
- Dependencies point inward: Api → Infrastructure → Application → Domain. Architecture tests enforce it.
- Expected failures are returned as `Result`, not thrown. Unexpected ones go to the
  global exception handler and become `500` problem details.
- Time comes from `TimeProvider`, so tests control the clock.
- Central package versions (`Directory.Packages.props`), warnings as errors and
  .NET analyzers (`Directory.Build.props`), code style in `.editorconfig`.

## Tests

```bash
cd backend && dotnet test     # 108 tests (xUnit v3 on Microsoft Testing Platform)
cd frontend && npm test       # 13 tests (Vitest + Testing Library)
```

Code coverage: `dotnet test --coverage`.

## Migrations

```bash
cd backend
dotnet tool restore
dotnet ef migrations add <Name> \
  -p src/Modules/Todos/TodoApp.Modules.Todos.Infrastructure \
  -s src/Bootstrapper/TodoApp.Api \
  -o Persistence/Migrations
```

For TemporaryTodos use `-p src/Modules/TemporaryTodos/TodoApp.Modules.TemporaryTodos.Infrastructure`.

Outside Development, migrations are not applied on start (`Modules:<Module>:Database:ApplyMigrationsOnStartup`).
Apply them as a deploy step instead, for example with `dotnet ef database update` or a migration bundle.
