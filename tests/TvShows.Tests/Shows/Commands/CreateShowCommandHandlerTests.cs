using NSubstitute;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Application.Shows.Commands.CreateShow;
using TvShows.Infrastructure;
using TvShows.Infrastructure.Shows.Commands;
using TvShows.Tests._support;

namespace TvShows.Tests.Shows.Commands;

public class CreateShowCommandHandlerTests
{
    private readonly IShowRepository _repo = Substitute.For<IShowRepository>();
    private readonly TestHybridCache _cache = new();

    [Fact]
    public async Task Persists_show_and_invalidates_cache()
    {
        var sut = new CreateShowCommandHandler(_repo, _cache);
        _repo.AddAsync(Arg.Any<Show>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Show>());

        var command = new CreateShowCommand(
            "Name", "English", new DateTime(2021, 1, 1), new List<string> { "Drama" }, "Summary");

        var result = await sut.Handle(command, default);

        Assert.Equal("Name", result.Name);
        await _repo.Received(1).AddAsync(
            Arg.Is<Show>(s => s.Name == "Name" && s.Language == "English"),
            Arg.Any<CancellationToken>());
        Assert.Contains(ShowCache.Tag, _cache.RemovedTags);
    }

    [Fact]
    public async Task Defaults_genres_to_empty_when_null()
    {
        var sut = new CreateShowCommandHandler(_repo, _cache);
        _repo.AddAsync(Arg.Any<Show>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Show>());

        var command = new CreateShowCommand("Name", null, null, null, null);

        var result = await sut.Handle(command, default);

        Assert.Empty(result.Genres);
    }
}
