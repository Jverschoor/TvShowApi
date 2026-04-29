namespace TvShows.Application.Shows;

public record ShowListItem(
    int Id,
    int? TvMazeId,
    string Name,
    string? Language,
    DateTime? Premiered,
    IReadOnlyList<string> Genres);
