using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using NeoSmart.Caching.Sqlite;
using TvShows.Application.Interfaces;
using TvShows.Infrastructure.Data;
using TvShows.Infrastructure.Repositories;
using TvShows.Infrastructure.Services;
using TvShows.Infrastructure.TvMaze;

namespace TvShows.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString = null)
    {
        var showsConnection = string.IsNullOrWhiteSpace(connectionString)
            ? SolutionPaths.DefaultShowsDbConnectionString()
            : connectionString;
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(showsConnection));
        services.AddScoped<IShowRepository, ShowRepository>();
        services.AddScoped<IShowSearchService, ShowSearchService>();
        services.AddScoped<IShowSyncService, ShowSyncService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddHttpClient<ITvMazeClient, TvMazeClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.tvmaze.com/");
        });

        services.AddSqliteCache(SolutionPaths.DefaultCacheDbConnectionString());
        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 32 * 1024 * 1024;
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(10),
                Expiration = TimeSpan.FromMinutes(10)
            };
        });

        return services;
    }
}
