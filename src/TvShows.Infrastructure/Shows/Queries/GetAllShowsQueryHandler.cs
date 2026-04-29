using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using TvShows.Application.Interfaces;
using TvShows.Application.Shows;
using TvShows.Application.Shows.Queries.GetAllShows;

namespace TvShows.Infrastructure.Shows.Queries;

public class GetAllShowsQueryHandler(IShowRepository repository, HybridCache cache) : IRequestHandler<GetAllShowsQuery, PagedResult<ShowListItem>>
{
    public async Task<PagedResult<ShowListItem>> Handle(GetAllShowsQuery request, CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(
            ShowCache.PageKey(request.Page, request.PageSize),
            async token => await repository.GetPagedAsync(request.Page, request.PageSize, token),
            tags: ShowCache.Tags,
            cancellationToken: cancellationToken);
    }
}
