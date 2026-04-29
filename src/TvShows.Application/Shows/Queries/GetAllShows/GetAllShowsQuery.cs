using MediatR;
using TvShows.Application.Shows;

namespace TvShows.Application.Shows.Queries.GetAllShows;

public record GetAllShowsQuery(int Page = 1, int PageSize = 50) : IRequest<PagedResult<ShowListItem>>;
