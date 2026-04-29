using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TvShows.Application.Entities;
using TvShows.Application.Interfaces;
using TvShows.Infrastructure;
using TvShows.Infrastructure.Data;
using TvShows.Infrastructure.Services;
using TvShows.Tests._support;

namespace TvShows.Tests.Services;

public class ShowSyncServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly ITvMazeClient _client = Substitute.For<ITvMazeClient>();
    private readonly TestHybridCache _cache = new();
    private readonly ShowSyncService _sut;

    public ShowSyncServiceTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _sut = new ShowSyncService(_db, _client, _cache, NullLogger<ShowSyncService>.Instance);
    }

    [Fact]
    public async Task IncrementalSync_upserts_new_shows_and_invalidates_cache()
    {
        _client.GetShowUpdatesAsync("day", Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, long> { { 1, 0 }, { 2, 0 } });
        _client.GetShowByTvMazeIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(ShowFixture.Show(tvMazeId: 1, premiered: new DateTime(2020, 1, 1), name: "One"));
        _client.GetShowByTvMazeIdAsync(2, Arg.Any<CancellationToken>())
            .Returns(ShowFixture.Show(tvMazeId: 2, premiered: new DateTime(2020, 1, 1), name: "Two"));

        await _sut.IncrementalSyncAsync();

        Assert.Equal(2, await _db.Shows.CountAsync());
        Assert.Contains(ShowCache.Tag, _cache.RemovedTags);
    }

    [Fact]
    public async Task IncrementalSync_filters_out_shows_premiered_before_cutoff()
    {
        _client.GetShowUpdatesAsync("day", Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, long> { { 1, 0 } });
        _client.GetShowByTvMazeIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(ShowFixture.Show(tvMazeId: 1, premiered: new DateTime(2010, 1, 1)));

        await _sut.IncrementalSyncAsync();

        Assert.Equal(0, await _db.Shows.CountAsync());
    }

    [Fact]
    public async Task IncrementalSync_skips_when_no_updates_and_does_not_invalidate_cache()
    {
        _client.GetShowUpdatesAsync("day", Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, long>());

        await _sut.IncrementalSyncAsync();

        Assert.Equal(0, await _db.Shows.CountAsync());
        Assert.Empty(_cache.RemovedTags);
    }

    [Fact]
    public async Task IncrementalSync_updates_existing_show_in_place()
    {
        _db.Shows.Add(new Show { TvMazeId = 1, Name = "Old", Premiered = new DateTime(2020, 1, 1), Genres = new() });
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        _client.GetShowUpdatesAsync("day", Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, long> { { 1, 0 } });
        _client.GetShowByTvMazeIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(ShowFixture.Show(tvMazeId: 1, premiered: new DateTime(2021, 6, 1), name: "New"));

        await _sut.IncrementalSyncAsync();

        var stored = await _db.Shows.SingleAsync();
        Assert.Equal("New", stored.Name);
        Assert.Equal(new DateTime(2021, 6, 1), stored.Premiered);
    }

    [Fact]
    public async Task IncrementalSync_skips_null_results_from_client_and_processes_others()
    {
        _client.GetShowUpdatesAsync("day", Arg.Any<CancellationToken>())
            .Returns(new Dictionary<int, long> { { 1, 0 }, { 2, 0 } });
        _client.GetShowByTvMazeIdAsync(1, Arg.Any<CancellationToken>()).Returns((Show?)null);
        _client.GetShowByTvMazeIdAsync(2, Arg.Any<CancellationToken>())
            .Returns(ShowFixture.Show(tvMazeId: 2, premiered: new DateTime(2020, 1, 1)));

        await _sut.IncrementalSyncAsync();

        Assert.Equal(1, await _db.Shows.CountAsync());
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
