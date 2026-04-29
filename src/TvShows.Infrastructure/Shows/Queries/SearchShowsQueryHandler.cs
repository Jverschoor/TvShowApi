using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Application.Shows.Queries.SearchShows;

namespace TvShows.Infrastructure.Shows.Queries;

public class SearchShowsQueryHandler(IShowSearchService searchService, HybridCache cache) : IRequestHandler<SearchShowsQuery, IReadOnlyList<Show>>
{
    public async Task<IReadOnlyList<Show>> Handle(SearchShowsQuery request, CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(
            ShowCache.SearchKey(request.Name),
            async token => await searchService.SearchAsync(request.Name, token),
            tags: ShowCache.Tags,
            cancellationToken: cancellationToken);
    }
}
