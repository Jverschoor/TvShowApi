using System.Text.Json.Serialization;

namespace TvShows.Infrastructure.TvMaze;

internal class TvMazeSearchResult
{
    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("show")]
    public TvMazeShow Show { get; set; } = null!;
}
