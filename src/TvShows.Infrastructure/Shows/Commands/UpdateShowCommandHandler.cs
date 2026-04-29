using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Application.Shows.Commands.UpdateShow;

namespace TvShows.Infrastructure.Shows.Commands;

public class UpdateShowCommandHandler(IShowRepository repository, HybridCache cache) : IRequestHandler<UpdateShowCommand, Show?>
{
    public async Task<Show?> Handle(UpdateShowCommand request, CancellationToken cancellationToken)
    {
        var show = await repository.GetByIdTrackingAsync(request.Id, cancellationToken);
        if (show is null) return null;

        show.Name = request.Name;
        show.Language = request.Language;
        show.Premiered = request.Premiered;
        show.Genres = request.Genres ?? [];
        show.Summary = request.Summary;

        await repository.UpdateAsync(show, cancellationToken);
        await cache.RemoveByTagAsync(ShowCache.Tag, cancellationToken);
        return show;
    }
}
