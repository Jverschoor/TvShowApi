using TvShows.Application.Entities;

namespace TvShows.Application.Interfaces;

public interface IShowSyncService
{
    Task FullSyncAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default);
    Task IncrementalSyncAsync(CancellationToken cancellationToken = default);
}
