using System.Diagnostics;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Infrastructure.Data;

namespace TvShows.Infrastructure.Services;

public class ShowSyncService(
    AppDbContext db,
    ITvMazeClient client,
    HybridCache cache,
    ILogger<ShowSyncService> logger) : IShowSyncService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan BatchWindow = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan RateLimitBackoff = TimeSpan.FromSeconds(15);

    public async Task FullSyncAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var currentPage = await GetResumePageAsync(cancellationToken);
        var anyUpserted = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            var batchStart = Stopwatch.GetTimestamp();
            var (successful, rateLimited) = await FetchPageBatchAsync(currentPage, cancellationToken);

            var contiguous = successful
                .OrderBy(p => p.Page)
                .TakeWhile((p, i) => p.Page == currentPage + i)
                .ToList();

            var reachedEnd = contiguous.Any(p => p.Shows is null);
            var shows = contiguous.TakeWhile(p => p.Shows is not null).SelectMany(p => p.Shows!).ToList();

            if (shows.Count > 0)
            {
                await UpsertShowsAsync(shows, cancellationToken);
                anyUpserted = true;
            }

            var pagesProcessed = contiguous.TakeWhile(p => p.Shows is not null).Count();
            if (pagesProcessed > 0)
            {
                var lastPage = currentPage + pagesProcessed - 1;
                progress?.Report($"Pages {currentPage}-{lastPage}: {shows.Count} shows processed");
                logger.LogInformation("Scraped pages {From}-{To}: {Count} shows processed",
                    currentPage, lastPage, shows.Count);
                currentPage += pagesProcessed;
            }

            if (reachedEnd)
            {
                progress?.Report("Full sync complete.");
                break;
            }

            if (rateLimited > 0)
            {
                progress?.Report($"Rate limited ({rateLimited} pages). Backing off for {RateLimitBackoff.TotalSeconds}s...");
                await Task.Delay(RateLimitBackoff, cancellationToken);
                continue;
            }

            var elapsed = Stopwatch.GetElapsedTime(batchStart);
            var remaining = BatchWindow - elapsed;
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining, cancellationToken);
        }

        if (anyUpserted)
            await cache.RemoveByTagAsync(ShowCache.Tag, cancellationToken);
    }

    public async Task IncrementalSyncAsync(CancellationToken cancellationToken = default)
    {
        var updates = await client.GetShowUpdatesAsync("day", cancellationToken);
        var showIds = updates.Keys.ToList();
        logger.LogInformation("Found {Count} updated shows to sync", showIds.Count);

        var anyUpserted = false;
        var i = 0;
        while (i < showIds.Count && !cancellationToken.IsCancellationRequested)
        {
            var batchStart = Stopwatch.GetTimestamp();
            var batchIds = showIds.GetRange(i, Math.Min(BatchSize, showIds.Count - i));

            var (shows, rateLimited) = await FetchShowsByIdAsync(batchIds, cancellationToken);

            if (shows.Count > 0)
            {
                await UpsertShowsAsync(shows, cancellationToken);
                anyUpserted = true;
            }

            logger.LogInformation("Incremental sync batch {From}-{To} of {Total}: {Count} shows upserted",
                i, i + batchIds.Count, showIds.Count, shows.Count);

            if (rateLimited)
            {
                logger.LogWarning("Rate limited. Backing off for {Seconds}s", RateLimitBackoff.TotalSeconds);
                await Task.Delay(RateLimitBackoff, cancellationToken);
                continue;
            }

            i += batchIds.Count;

            var elapsed = Stopwatch.GetElapsedTime(batchStart);
            var remaining = BatchWindow - elapsed;
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining, cancellationToken);
        }

        if (anyUpserted)
            await cache.RemoveByTagAsync(ShowCache.Tag, cancellationToken);
    }

    private async Task<int> GetResumePageAsync(CancellationToken ct)
    {
        var maxId = await db.Shows.MaxAsync(s => (int?)s.TvMazeId, ct) ?? 0;
        return maxId / 250;
    }

    private async Task<(List<Show> Shows, bool RateLimited)> FetchShowsByIdAsync(
        List<int> tvMazeIds, CancellationToken ct)
    {
        var tasks = tvMazeIds.Select(id => FetchSingleShowAsync(id, ct));
        var results = await Task.WhenAll(tasks);

        var shows = results.Where(r => r.Show is not null).Select(r => r.Show!).ToList();
        var rateLimited = results.Any(r => r.RateLimited);
        return (shows, rateLimited);
    }

    private async Task<(Show? Show, bool RateLimited)> FetchSingleShowAsync(int tvMazeId, CancellationToken ct)
    {
        try
        {
            var show = await client.GetShowByTvMazeIdAsync(tvMazeId, ct);
            return (show, false);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return (null, true);
        }
    }

    private async Task<(List<(int Page, IReadOnlyList<Show>? Shows)> Successful, int RateLimited)> FetchPageBatchAsync(
        int startPage, CancellationToken ct)
    {
        var tasks = Enumerable.Range(startPage, BatchSize).Select(page => FetchPageAsync(page, ct));
        var results = await Task.WhenAll(tasks);

        var successful = results
            .Where(r => !r.RateLimited)
            .Select(r => (r.Page, r.Shows))
            .ToList();
        var rateLimited = results.Count(r => r.RateLimited);
        return (successful, rateLimited);
    }

    private async Task<(int Page, IReadOnlyList<Show>? Shows, bool RateLimited)> FetchPageAsync(
        int page, CancellationToken ct)
    {
        try
        {
            var shows = await client.GetShowsByPageAsync(page, ct);
            return (page, shows, false);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return (page, null, true);
        }
    }

    private async Task UpsertShowsAsync(IReadOnlyList<Show> shows, CancellationToken ct)
    {
        var eligible = shows
            .Where(s => s.TvMazeId is not null && s.Premiered > Show.PremieredCutoff)
            .GroupBy(s => s.TvMazeId!.Value)
            .Select(g => g.Last())
            .ToList();

        if (eligible.Count == 0)
            return;

        var ids = eligible.Select(s => s.TvMazeId!.Value).ToList();
        var existing = await db.Shows
            .Where(s => s.TvMazeId != null && ids.Contains(s.TvMazeId.Value))
            .ToDictionaryAsync(s => s.TvMazeId!.Value, ct);

        foreach (var show in eligible)
        {
            if (existing.TryGetValue(show.TvMazeId!.Value, out var current))
            {
                current.Name = show.Name;
                current.Language = show.Language;
                current.Premiered = show.Premiered;
                current.Genres = show.Genres;
                current.Summary = show.Summary;
            }
            else
            {
                db.Shows.Add(show);
            }
        }

        await db.SaveChangesAsync(ct);
        db.ChangeTracker.Clear();
    }
}
