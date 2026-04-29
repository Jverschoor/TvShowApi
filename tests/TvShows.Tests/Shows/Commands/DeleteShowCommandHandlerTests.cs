using NSubstitute;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Application.Shows.Commands.DeleteShow;
using TvShows.Infrastructure;
using TvShows.Infrastructure.Shows.Commands;
using TvShows.Tests._support;

namespace TvShows.Tests.Shows.Commands;

public class DeleteShowCommandHandlerTests
{
    private readonly IShowRepository _repo = Substitute.For<IShowRepository>();
    private readonly TestHybridCache _cache = new();

    [Fact]
    public async Task Deletes_existing_show_and_invalidates_cache()
    {
        var show = ShowFixture.Show();
        show.Id = 1;
        _repo.GetByIdTrackingAsync(1, Arg.Any<CancellationToken>()).Returns(show);
        var sut = new DeleteShowCommandHandler(_repo, _cache);

        var result = await sut.Handle(new DeleteShowCommand(1), default);

        Assert.True(result);
        await _repo.Received(1).DeleteAsync(show, Arg.Any<CancellationToken>());
        Assert.Contains(ShowCache.Tag, _cache.RemovedTags);
    }

    [Fact]
    public async Task Returns_false_when_show_not_found()
    {
        _repo.GetByIdTrackingAsync(42, Arg.Any<CancellationToken>()).Returns((Show?)null);
        var sut = new DeleteShowCommandHandler(_repo, _cache);

        var result = await sut.Handle(new DeleteShowCommand(42), default);

        Assert.False(result);
        await _repo.DidNotReceive().DeleteAsync(Arg.Any<Show>(), Arg.Any<CancellationToken>());
        Assert.Empty(_cache.RemovedTags);
    }
}
