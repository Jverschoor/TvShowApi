using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TvShows.Application.Interfaces;
using TvShows.Infrastructure;
using TvShows.Infrastructure.Data;

var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

var services = new ServiceCollection();
services.AddInfrastructure(connectionString);
services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));

await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.Database.EnsureCreatedAsync();

var startCount = await db.Shows.CountAsync();
Console.WriteLine(startCount > 0
    ? $"Database contains {startCount} shows. Resuming full sync from TVMaze..."
    : "Database is empty. Starting full sync from TVMaze...");

var syncService = scope.ServiceProvider.GetRequiredService<IShowSyncService>();
var progress = new Progress<string>(Console.WriteLine);
await syncService.FullSyncAsync(progress);

var totalShows = await db.Shows.CountAsync();
Console.WriteLine($"Full sync complete. {totalShows} shows in database.");
