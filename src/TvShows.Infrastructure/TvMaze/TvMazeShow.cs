using System.Globalization;
using System.Text.Json.Serialization;
using TvShows.Application.Entities;

namespace TvShows.Infrastructure.TvMaze;

internal class TvMazeShow
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("premiered")]
    public string? Premiered { get; set; }

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = [];

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    public Show ToShow() => new()
    {
        TvMazeId = Id,
        Name = Name,
        Language = Language,
        Premiered = DateTime.TryParse(Premiered, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null,
        Genres = Genres,
        Summary = Summary
    };
}
