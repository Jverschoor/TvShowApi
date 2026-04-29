# TvShows Tech Test - TV Show Scraper API

## Assignment Requirements

### Core Requirements
- Build an ASP.NET Core Web API
- Fetch show data from http://www.tvmaze.com/api
- Only store shows premiered after 2014-01-01 (strictly after, not on)
- Store the following fields per show:
  - Id
  - Name
  - Language
  - Premiered
  - Genres
  - Summary

### CRUD Operations
- Add new shows that are not on TVMaze
- Search for a show by name
- List all shows
- Update all information of a show by id
- Delete a show by id

### Technical Requirements
- ASP.NET Core Web API project
- An ORM
- TVMaze API as external data source
- A database of your choice
- Project must be easy to run
- Handle TVMaze API pagination and rate limiting

### Testing & Quality
- Code should be readable
- Good coding style
- Unit tests

### Bonus Points
- Sufficient amount of unit tests
- Caching mechanism
- Shows displayed in descending order of premiered date
- Scraper resumes where it left off after shutdown
- Combine show data with cast data from another TVMaze source
- Dependency Injection to separate controllers from data
- Clean Architecture in project structure

## Code Style
- Use primary constructors for dependency injection (e.g. `public class Foo(IBar bar) : Base`)

## Architecture Decisions

| Decision | Choice | Rationale |
|---|---|---|
| Architecture | 4 projects (Api, Application, Infrastructure, Tests) | Clean Architecture |
| Database | SQLite + EF Core | Zero setup, easy to run |
| Scraper | Console app (full seed) + Azure Function (nightly incremental) | Separated concerns: one-time seed vs daily updates |
| Genres | JSON column in EF Core | Simpler than join table for read-mostly data |
| Primary Key | Local auto-increment PK + nullable TvMazeId | Allows adding shows not on TVMaze |
| Rate Limiting | Simple delay between TVMaze requests | TVMaze allows ~20 req/10s |
| Caching | IMemoryCache on GET endpoints | Lightweight, no external dependencies |
| Bonus scope | All except cast data | Realistic within time budget |

## Project Structure
```
TvShows.slnx
src/
  TvShows.Api/              - Controllers, Program.cs, Swagger config
  TvShows.Application/      - Entities, interfaces, CQRS commands/queries (no dependencies except MediatR)
    Entities/             - Show.cs
    Interfaces/           - IShowRepository.cs, IShowSyncService.cs, ITvMazeClient.cs
    Shows/
      Commands/           - CreateShow, UpdateShow, DeleteShow (command records)
      Queries/            - GetAllShows, GetShowById, SearchShows (query records)
  TvShows.Console/          - Console app for initial full DB seed from TVMaze
  TvShows.Functions/        - Azure Function with nightly timer trigger for incremental sync
  TvShows.Infrastructure/   - EF Core, repository, handlers, DI registration
    Data/                 - AppDbContext.cs
    Repositories/         - ShowRepository.cs
    Services/             - ShowSyncService.cs, ShowSearchService.cs
    TvMaze/               - TvMazeClient.cs, TvMazeShow.cs
    Shows/
      Commands/           - Command handlers
      Queries/            - Query handlers
tests/
  TvShows.Tests/            - xUnit test project
docs/
  v1.yaml                 - TVMaze user API spec (not useful — Show is untyped)
```

Dependencies: Api/Console/Functions → Application + Infrastructure, Infrastructure → Application, Tests → all.

## Current Progress

### Done
- [x] Solution scaffolded with 4 projects (Api, Application, Infrastructure, Tests)
- [x] Show entity with local PK + nullable TvMazeId, JSON-serialized Genres
- [x] CQRS with MediatR: commands (Create, Update, Delete) and queries (GetAll, GetById, Search)
- [x] All handlers in Infrastructure layer, using primary constructors
- [x] IShowRepository interface in Application, ShowRepository in Infrastructure
- [x] Explicit tracking split: GetByIdAsync (untracked reads) vs GetByIdTrackingAsync (mutations)
- [x] AppDbContext (SQLite) with indexes on Premiered, TvMazeId (unique filtered)
- [x] ShowsController dispatches via IMediator, all endpoints: GET all, GET search, GET by id, POST, PUT, DELETE
- [x] DI registration via Infrastructure.DependencyInjection extension method
- [x] EnsureCreatedAsync on startup — DB auto-created, zero setup for reviewers
- [x] Swagger UI configured, launches on F5
- [x] .gitignore for .NET, SQLite, IDE files
- [x] appsettings.json with connection string (Data Source=shows.db)
- [x] TVMaze sync: Console app for full seed, Azure Function for nightly incremental updates via /updates/shows?since=day
- [x] Caching (HybridCache with SQLite L2 on GET endpoints, invalidated by scraper and commands)

### TODO
- [ ] Unit tests
- [ ] README with run instructions and architecture explanation
