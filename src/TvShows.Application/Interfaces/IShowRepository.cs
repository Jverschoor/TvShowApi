using TvShows.Application.Entities;
using TvShows.Application.Shows;

namespace TvShows.Application.Interfaces;

public interface IShowRepository
{
    Task<PagedResult<ShowListItem>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Show>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
Task<Show?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Show?> GetByTvMazeIdAsync(int tvMazeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Show>> GetByTvMazeIdsAsync(IList<int> ids, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Show> shows, CancellationToken cancellationToken = default);
    Task<Show?> GetByIdTrackingAsync(int id, CancellationToken cancellationToken = default);
    Task<Show> AddAsync(Show show, CancellationToken cancellationToken = default);
    Task UpdateAsync(Show show, CancellationToken cancellationToken = default);
    Task DeleteAsync(Show show, CancellationToken cancellationToken = default);
}
