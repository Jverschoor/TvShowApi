using TvShows.Application.Entities;

namespace TvShows.Application.Interfaces;

public interface ITvMazeClient
{
    Task<IReadOnlyList<Show>> SearchShowsAsync(string query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Show>?> GetShowsByPageAsync(int page, CancellationToken cancellationToken = default);
    Task<Dictionary<int, long>> GetShowUpdatesAsync(string since, CancellationToken cancellationToken = default);
    Task<Show?> GetShowByTvMazeIdAsync(int tvMazeId, CancellationToken cancellationToken = default);
}
