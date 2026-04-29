using MediatR;
using TvShows.Application.Entities;

namespace TvShows.Application.Shows.Queries.GetShowById;

public record GetShowByIdQuery(int Id) : IRequest<Show?>;
