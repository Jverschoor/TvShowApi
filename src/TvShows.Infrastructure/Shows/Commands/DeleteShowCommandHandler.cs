using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using TvShows.Application.Interfaces;
using TvShows.Application.Shows.Commands.DeleteShow;

namespace TvShows.Infrastructure.Shows.Commands;

public class DeleteShowCommandHandler(IShowRepository repository, HybridCache cache) : IRequestHandler<DeleteShowCommand, bool>
{
    public async Task<bool> Handle(DeleteShowCommand request, CancellationToken cancellationToken)
    {
        var show = await repository.GetByIdTrackingAsync(request.Id, cancellationToken);
        if (show is null) return false;

        await repository.DeleteAsync(show, cancellationToken);
        await cache.RemoveByTagAsync(ShowCache.Tag, cancellationToken);
        return true;
    }
}
