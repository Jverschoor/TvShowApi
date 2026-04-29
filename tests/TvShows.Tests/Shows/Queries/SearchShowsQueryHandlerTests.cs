using NSubstitute;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Application.Shows.Queries.SearchShows;
using TvShows.Infrastructure.Shows.Queries;
using TvShows.Tests._support;

namespace TvShows.Tests.Shows.Queries;

public class SearchShowsQueryHandlerTests
{
    [Fact]
    public async Task Delegates_to_search_service()
    {
        var service = Substitute.For<IShowSearchService>();
        var expected = new List<Show> { ShowFixture.Show(tvMazeId: 1) };
        service.SearchAsync("foo", Arg.Any<CancellationToken>()).Returns(expected);
        var sut = new SearchShowsQueryHandler(service, new TestHybridCache());

        var result = await sut.Handle(new SearchShowsQuery("foo"), default);

        Assert.Same(expected, result);
        await service.Received(1).SearchAsync("foo", Arg.Any<CancellationToken>());
    }
}
