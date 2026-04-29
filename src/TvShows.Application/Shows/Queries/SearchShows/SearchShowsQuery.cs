using MediatR;
using TvShows.Application.Entities;

namespace TvShows.Application.Shows.Queries.SearchShows;

public record SearchShowsQuery(string Name) : IRequest<IReadOnlyList<Show>>;
