using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Infrastructure.Services;
using TvShows.Tests._support;

namespace TvShows.Tests.Services;

public class ShowSearchServiceTests
{
    private readonly IShowRepository _repo = Substitute.For<IShowRepository>();
    private readonly ITvMazeClient _tvMaze = Substitute.For<ITvMazeClient>();
    private readonly ShowSearchService _sut;

    public ShowSearchServiceTests()
    {
        _sut = new ShowSearchService(_repo, _tvMaze, NullLogger<ShowSearchService>.Instance);
    }

    [Fact]
    public async Task Returns_db_results_only_when_threshold_met()
    {
        var dbShows = Enumerable.Range(1, 10).Select(i => ShowFixture.Show(tvMazeId: i)).ToList();
        _repo.SearchByNameAsync("foo", Arg.Any<CancellationToken>()).Returns(dbShows);

        var result = await _sut.SearchAsync("foo");

        Assert.Equal(10, result.Count);
        await _tvMaze.DidNotReceive().SearchShowsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Merges_tvmaze_results_filtering_duplicates_and_old_premieres()
    {
        var dbShows = new List<Show> { ShowFixture.Show(tvMazeId: 1, premiered: new DateTime(2020, 1, 1)) };
        var tvMazeShows = new List<Show>
        {
            ShowFixture.Show(tvMazeId: 1, premiered: new DateTime(2020, 1, 1)),  // duplicate
            ShowFixture.Show(tvMazeId: 2, premiered: new DateTime(2010, 1, 1)),  // before cutoff
            ShowFixture.Show(tvMazeId: 3, premiered: new DateTime(2021, 1, 1)),  // new and valid
            ShowFixture.Show(tvMazeId: null, premiered: new DateTime(2021, 1, 1))  // no TvMazeId
        };
        _repo.SearchByNameAsync("foo", Arg.Any<CancellationToken>()).Returns(dbShows);
        _repo.GetByTvMazeIdsAsync(Arg.Any<IList<int>>(), Arg.Any<CancellationToken>()).Returns(dbShows);
        _tvMaze.SearchShowsAsync("foo", Arg.Any<CancellationToken>()).Returns(tvMazeShows);

        var result = await _sut.SearchAsync("foo");

        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.TvMazeId == 1);
        Assert.Contains(result, s => s.TvMazeId == 3);
        await _repo.Received(1).AddRangeAsync(
            Arg.Is<List<Show>>(list => list.Count == 1 && list[0].TvMazeId == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_db_results_when_tvmaze_throws_http_exception()
    {
        var dbShows = new List<Show> { ShowFixture.Show(tvMazeId: 1) };
        _repo.SearchByNameAsync("foo", Arg.Any<CancellationToken>()).Returns(dbShows);
        _tvMaze.SearchShowsAsync("foo", Arg.Any<CancellationToken>())
            .Throws(new HttpRequestException("boom"));

        var result = await _sut.SearchAsync("foo");

        Assert.Single(result);
        await _repo.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<Show>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_call_repo_add_when_no_new_shows_to_persist()
    {
        var dbShows = new List<Show> { ShowFixture.Show(tvMazeId: 1) };
        _repo.SearchByNameAsync("foo", Arg.Any<CancellationToken>()).Returns(dbShows);
        _repo.GetByTvMazeIdsAsync(Arg.Any<IList<int>>(), Arg.Any<CancellationToken>()).Returns(dbShows);
        _tvMaze.SearchShowsAsync("foo", Arg.Any<CancellationToken>())
            .Returns(new List<Show> { ShowFixture.Show(tvMazeId: 1) });

        await _sut.SearchAsync("foo");

        await _repo.DidNotReceive().AddRangeAsync(Arg.Any<IEnumerable<Show>>(), Arg.Any<CancellationToken>());
    }
}
