using MediatR;
using TvShows.Application.Entities;

namespace TvShows.Application.Shows.Commands.UpdateShow;

public record UpdateShowCommand(
    int Id,
    string Name,
    string? Language,
    DateTime? Premiered,
    List<string>? Genres,
    string? Summary) : IRequest<Show?>;
