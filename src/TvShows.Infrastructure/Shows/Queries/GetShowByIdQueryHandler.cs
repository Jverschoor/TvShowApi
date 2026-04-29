using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Application.Shows.Queries.GetShowById;

namespace TvShows.Infrastructure.Shows.Queries;

public class GetShowByIdQueryHandler(IShowRepository repository, HybridCache cache) : IRequestHandler<GetShowByIdQuery, Show?>
{
    public async Task<Show?> Handle(GetShowByIdQuery request, CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync(
            ShowCache.ByIdKey(request.Id),
            async token => await repository.GetByIdAsync(request.Id, token),
            tags: ShowCache.Tags,
            cancellationToken: cancellationToken);
    }
}
