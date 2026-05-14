Add a new API endpoint to the EnrollMate API following the established pattern.

## Project context

Endpoints are grouped into static classes under `src/EnrollMate.Api/Endpoints/`:
- `ApplicationsEndpoints.cs` — CRUD for enrollment applications
- `AgentEndpoints.cs` — trigger and query the AI agent
- `NotificationsEndpoints.cs` — notification-related operations

Each class has a single `Map(WebApplication app)` static method that registers all routes for that group using `app.MapGet(...)`, `app.MapPost(...)`, etc. Routes use minimal API handlers (lambdas), not controllers.

The `Map` method for each class is called from `Program.cs`. If a brand-new endpoint group file is created, it must also be called there.

Repositories are resolved from DI inside the lambda via the `HttpContext` or directly injected as parameters. Look at existing endpoints for the exact pattern used.

JSON enum serialization is configured globally in `Program.cs` — enums are serialized as strings, not integers.

## What to do

The user wants to add this endpoint: $ARGUMENTS

1. Decide which endpoint class it belongs to, or whether a new class is needed.
2. Read the relevant endpoint file before editing.
3. Add the route using the same style (method, path convention, lambda signature) as the existing routes in that file.
4. If a new file is needed, create it following the same static class + `Map` pattern, then add the `Map` call in `Program.cs`.
5. Return meaningful HTTP status codes — 404 for not found, 400 for bad input, 200/201 for success.
6. Do not add controllers, attributes, or MediatR — this codebase uses minimal APIs only.
