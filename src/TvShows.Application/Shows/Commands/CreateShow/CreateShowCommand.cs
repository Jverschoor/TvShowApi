using MediatR;
using TvShows.Application.Entities;

namespace TvShows.Application.Shows.Commands.CreateShow;

public record CreateShowCommand(
    string Name,
    string? Language,
    DateTime? Premiered,
    List<string>? Genres,
    string? Summary) : IRequest<Show>;
