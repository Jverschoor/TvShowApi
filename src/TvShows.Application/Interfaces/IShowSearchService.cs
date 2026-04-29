using TvShows.Application.Entities;

namespace TvShows.Application.Interfaces;

public interface IShowSearchService
{
    Task<IReadOnlyList<Show>> SearchAsync(string name, CancellationToken cancellationToken = default);
}
