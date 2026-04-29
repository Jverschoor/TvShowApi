using MediatR;

namespace TvShows.Application.Shows.Commands.DeleteShow;

public record DeleteShowCommand(int Id) : IRequest<bool>;
