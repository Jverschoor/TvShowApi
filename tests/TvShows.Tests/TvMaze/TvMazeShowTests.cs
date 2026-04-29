using TvShows.Infrastructure.TvMaze;

namespace TvShows.Tests.TvMaze;

public class TvMazeShowTests
{
    private static TvMazeShow Build(string? premiered) => new()
    {
        Id = 1,
        Name = "X",
        Premiered = premiered,
        Genres = new List<string>()
    };

    [Fact]
    public void ToShow_parses_valid_premiered_date()
    {
        Assert.Equal(new DateTime(2015, 4, 12), Build("2015-04-12").ToShow().Premiered);
    }

    [Fact]
    public void ToShow_returns_null_premiered_when_input_is_null()
    {
        Assert.Null(Build(null).ToShow().Premiered);
    }

    [Fact]
    public void ToShow_returns_null_premiered_when_input_is_empty()
    {
        Assert.Null(Build("").ToShow().Premiered);
    }

    [Fact]
    public void ToShow_returns_null_premiered_when_input_is_unparseable()
    {
        Assert.Null(Build("not-a-date").ToShow().Premiered);
    }
}
