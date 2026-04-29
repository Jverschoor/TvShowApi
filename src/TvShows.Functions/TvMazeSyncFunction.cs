using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TvShows.Application.Interfaces;

namespace TvShows.Functions;

public class TvMazeSyncFunction(IShowSyncService syncService, ILogger<TvMazeSyncFunction> logger)
{
    [Function("TvMazeSync")]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timer, CancellationToken cancellationToken)
    {
        logger.LogInformation("TVMaze sync triggered at {Time}", DateTime.UtcNow);

        await syncService.IncrementalSyncAsync(cancellationToken);

        logger.LogInformation("TVMaze sync completed");
    }
}
