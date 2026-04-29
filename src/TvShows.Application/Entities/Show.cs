namespace TvShows.Application.Entities;

public class Show
{
    public static readonly DateTime PremieredCutoff = new(2014, 1, 1);

    public int Id { get; set; }
    public int? TvMazeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Language { get; set; }
    public DateTime? Premiered { get; set; }
    public List<string> Genres { get; set; } = [];
    public string? Summary { get; set; }
}
