# TvShows API

ASP.NET Core Web API that scrapes shows from [TVMaze](https://www.tvmaze.com/api) and exposes CRUD operations over them. Only shows premiered after 2014-01-01 are stored.

## Running

The solution is built on .NET 9 and SQLite — no external setup required. The database file is created automatically on first run.

### API
Run `TvShows.Api` (F5 in your IDE or `dotnet run --project src/TvShows.Api`). Swagger opens automatically at:

- https://localhost:7276/swagger
- http://localhost:5093/swagger

All endpoints can be exercised from the Swagger UI.

### Console (initial seed)
Run `TvShows.Console` to populate the local database from TVMaze. It pages through `/shows`, respects rate limits, and resumes where it left off if interrupted.

```
dotnet run --project src/TvShows.Console
```

### Functions (incremental sync)
`TvShows.Functions` contains a timer-triggered Azure Function that hits TVMaze's `/updates/shows?since=day` endpoint nightly to keep the database current. It can also be invoked manually for an on-demand refresh.

```
func start --csharp --script-root src/TvShows.Functions
```

## Architecture

Clean Architecture across four projects:

| Project | Role |
|---|---|
| `TvShows.Api` | Controllers, DI bootstrap, Swagger |
| `TvShows.Application` | Entities, interfaces, CQRS commands/queries (MediatR) |
| `TvShows.Infrastructure` | EF Core, repositories, TVMaze client, command/query handlers |
| `TvShows.Console` | One-shot full seed |
| `TvShows.Functions` | Nightly incremental sync |
| `TvShows.Tests` | xUnit unit tests |

Key choices:
- **SQLite + EF Core** for zero-setup persistence
- **MediatR / CQRS** to keep controllers thin
- **HybridCache** (in-memory + SQLite L2) on read endpoints, invalidated by writes and the scraper
- **Genres** stored as a JSON column rather than a join table
- **Local auto-increment PK + nullable `TvMazeId`** so shows not on TVMaze can be added manually

## Tests

```
dotnet test
```
