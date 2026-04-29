using NSubstitute;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Application.Shows.Commands.UpdateShow;
using TvShows.Infrastructure;
using TvShows.Infrastructure.Shows.Commands;
using TvShows.Tests._support;

namespace TvShows.Tests.Shows.Commands;

public class UpdateShowCommandHandlerTests
{
    private readonly IShowRepository _repo = Substitute.For<IShowRepository>();
    private readonly TestHybridCache _cache = new();

    [Fact]
    public async Task Updates_existing_show_and_invalidates_cache()
    {
        var existing = ShowFixture.Show(tvMazeId: 99);
        existing.Id = 1;
        _repo.GetByIdTrackingAsync(1, Arg.Any<CancellationToken>()).Returns(existing);
        var sut = new UpdateShowCommandHandler(_repo, _cache);

        var command = new UpdateShowCommand(
            1, "Updated", "Spanish", new DateTime(2022, 1, 1), new List<string> { "Comedy" }, "New summary");

        var result = await sut.Handle(command, default);

        Assert.NotNull(result);
        Assert.Equal("Updated", result!.Name);
        Assert.Equal("Spanish", result.Language);
        await _repo.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
        Assert.Contains(ShowCache.Tag, _cache.RemovedTags);
    }

    [Fact]
    public async Task Returns_null_and_skips_update_when_show_not_found()
    {
        _repo.GetByIdTrackingAsync(42, Arg.Any<CancellationToken>()).Returns((Show?)null);
        var sut = new UpdateShowCommandHandler(_repo, _cache);

        var result = await sut.Handle(
            new UpdateShowCommand(42, "X", null, null, null, null), default);

        Assert.Null(result);
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Show>(), Arg.Any<CancellationToken>());
        Assert.Empty(_cache.RemovedTags);
    }
}
