using System.Net;
using System.Text;
using TvShows.Infrastructure.TvMaze;

namespace TvShows.Tests.TvMaze;

public class TvMazeClientTests
{
    private static TvMazeClient CreateClient(StubHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://api.tvmaze.com/") });

    [Fact]
    public async Task GetShowByTvMazeIdAsync_returns_null_on_404()
    {
        var client = CreateClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)));

        Assert.Null(await client.GetShowByTvMazeIdAsync(999));
    }

    [Fact]
    public async Task GetShowByTvMazeIdAsync_returns_mapped_show_on_success()
    {
        const string json = """
        {"id":42,"name":"Foo","language":"English","premiered":"2020-05-01","genres":["Drama"],"summary":"S"}
        """;
        var client = CreateClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        }));

        var result = await client.GetShowByTvMazeIdAsync(42);

        Assert.NotNull(result);
        Assert.Equal(42, result!.TvMazeId);
        Assert.Equal("Foo", result.Name);
        Assert.Equal(new DateTime(2020, 5, 1), result.Premiered);
    }

    [Fact]
    public async Task GetShowByTvMazeIdAsync_throws_on_server_error()
    {
        var client = CreateClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetShowByTvMazeIdAsync(1));
    }

    [Fact]
    public async Task SearchShowsAsync_returns_empty_when_no_results()
    {
        var client = CreateClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, "application/json")
        }));

        Assert.Empty(await client.SearchShowsAsync("anything"));
    }

    [Fact]
    public async Task GetShowsByPageAsync_returns_null_on_404_marking_pagination_end()
    {
        var client = CreateClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)));

        Assert.Null(await client.GetShowsByPageAsync(9999));
    }

    [Fact]
    public async Task GetShowsByPageAsync_throws_on_429_so_caller_can_back_off()
    {
        var client = CreateClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests)));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => client.GetShowsByPageAsync(1));
        Assert.Equal(HttpStatusCode.TooManyRequests, ex.StatusCode);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(respond(request));
    }
}
