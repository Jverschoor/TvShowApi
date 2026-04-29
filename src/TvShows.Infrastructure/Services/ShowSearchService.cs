using Microsoft.Extensions.Logging;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;

namespace TvShows.Infrastructure.Services;

public class ShowSearchService(
    IShowRepository repository,
    ITvMazeClient tvMaze,
    ILogger<ShowSearchService> logger) : IShowSearchService
{
    private const int MinResultsBeforeApiCall = 10;

    public async Task<IReadOnlyList<Show>> SearchAsync(string name, CancellationToken cancellationToken = default)
    {
        var dbResults = await repository.SearchByNameAsync(name, cancellationToken);
        if (dbResults.Count >= MinResultsBeforeApiCall)
            return dbResults;

        IReadOnlyList<Show> tvResults;
        try
        {
            tvResults = await tvMaze.SearchShowsAsync(name, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "TVMaze API unavailable, returning DB results only");
            return dbResults;
        }

        var candidateIds = tvResults
            .Where(s => s.TvMazeId.HasValue && s.Premiered > Show.PremieredCutoff)
            .Select(s => s.TvMazeId!.Value)
            .Distinct()
            .ToList();

        var existingTvMazeIds = candidateIds.Count == 0
            ? []
            : (await repository.GetByTvMazeIdsAsync(candidateIds, cancellationToken))
                .Where(s => s.TvMazeId.HasValue)
                .Select(s => s.TvMazeId!.Value)
                .ToHashSet();

        var toAdd = tvResults
            .Where(s => s.TvMazeId.HasValue
                && s.Premiered > Show.PremieredCutoff
                && !existingTvMazeIds.Contains(s.TvMazeId!.Value))
            .GroupBy(s => s.TvMazeId!.Value)
            .Select(g => g.First())
            .ToList();

        if (toAdd.Count > 0)
            await repository.AddRangeAsync(toAdd, cancellationToken);

        return dbResults
            .Concat(toAdd)
            .OrderByDescending(s => s.Premiered)
            .ToList();
    }
}
