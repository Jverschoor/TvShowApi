using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Application.Shows.Commands.CreateShow;

namespace TvShows.Infrastructure.Shows.Commands;

public class CreateShowCommandHandler(IShowRepository repository, HybridCache cache) : IRequestHandler<CreateShowCommand, Show>
{
    public async Task<Show> Handle(CreateShowCommand request, CancellationToken cancellationToken)
    {
        var show = new Show
        {
            Name = request.Name,
            Language = request.Language,
            Premiered = request.Premiered,
            Genres = request.Genres ?? [],
            Summary = request.Summary
        };

        var created = await repository.AddAsync(show, cancellationToken);
        await cache.RemoveByTagAsync(ShowCache.Tag, cancellationToken);
        return created;
    }
}
